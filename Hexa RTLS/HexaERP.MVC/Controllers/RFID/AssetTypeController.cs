using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssetTypeController : Controller
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

        //Get All Department
        [HttpGet]
        public JsonResult getData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            try
            {
                var ObjData = db.mIteamTypeMasters
                    .Where(o => o.OrgInfoId == orgId && o.IsAction == true)
                    .Select(o => new
                    {
                        mIteamTypeMasterId = o.mIteamTypeMasterId,
                        IteamType = o.IteamType
                    })
                    .ToList();
                //Convert List Data to The Json Array          
                return Json(ObjData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //Save New Departmnt
        [HttpPost]
        public string SaveData(string _IteamType)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            mIteamTypeMaster obj = new mIteamTypeMaster();
            string msg = "";
            try
            {
                if (db.mIteamTypeMasters.Any(o => o.IteamType == _IteamType && o.OrgInfoId == orgId))
                {
                    return msg = "Same Name Alerdy Exist";
                }
                else
                {
                    obj.OrgInfoId = orgId;
                    obj.IteamType = _IteamType;
                    obj.CreatedDate = DateTime.Now; obj.CreatedBy = UserName.ToString(); obj.IsAction = true;
                    db.mIteamTypeMasters.Add(obj);
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
            var datas = db.mIteamTypeMasters
                .Where(o => o.mIteamTypeMasterId == ID && o.OrgInfoId == orgId)
                .Select(o => new
                {
                    mIteamTypeMasterId = o.mIteamTypeMasterId,
                    IteamType = o.IteamType
                })
                .ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateData(string _IteamType, int ID)
        {
            string msg = "";
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.mIteamTypeMasters.FirstOrDefault(b => b.mIteamTypeMasterId == ID);
            if (original != null)
            {
                original.IteamType = _IteamType; original.ModifiedBy = UserName.ToString(); original.ModifiedDate = DateTime.Now;
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
            mIteamTypeMaster removeData = db.mIteamTypeMasters.Find(ID);
            if (removeData != null)
            {
                db.mIteamTypeMasters.Remove(removeData);
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
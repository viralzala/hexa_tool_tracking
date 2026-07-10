using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssetMasterController : Controller
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
            var ObjData = db.TC_LeadType.Where(o => o.OrgInfoId == orgId).ToList();
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
            var ObjData = db.mIteamMasters.Where(o => o.OrgInfoId == orgId && o.IsAction == true).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Save New Departmnt
        [HttpPost]
        public string SaveData(string _IteamName, string _IteamCode)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            mIteamMaster obj = new mIteamMaster();
            string msg = "";
            try
            {
                if (db.mIteamMasters.Any(o => o.IteamName == _IteamName && o.IteamCode == _IteamCode && o.OrgInfoId == orgId))
                {
                    return msg = "Same Name Alerdy Exist";
                }
                else
                {
                    obj.OrgInfoId = orgId;
                    obj.IteamName = _IteamName;
                    obj.IteamCode = _IteamCode;
                    obj.CreatedDate = DateTime.Now; obj.CreatedBy = UserName.ToString(); obj.IsAction = true;
                    db.mIteamMasters.Add(obj);
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
            var datas = db.mIteamMasters.Where(o => o.mIteamMasterId == ID && o.OrgInfoId == orgId).ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateData(string _IteamName, string _IteamCode, int ID)
        {
            string msg = "";
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.mIteamMasters.FirstOrDefault(b => b.mIteamMasterId == ID);
            if (original != null)
            {
                original.IteamName = _IteamName; original.IteamCode = _IteamCode; original.ModifiedBy = UserName.ToString(); original.ModifiedDate = DateTime.Now;
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
            mIteamMaster removeData = db.mIteamMasters.Find(ID);
            if (removeData != null)
            {
                db.mIteamMasters.Remove(removeData);
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
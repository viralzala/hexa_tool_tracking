using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class AssetSubCategory2Controller : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        public ActionResult Index()
        {
            try
            {
                HttpCookie cookieObject = Request.Cookies["HexaCookie"];
                if (cookieObject != null) { }
                else { return RedirectToAction("Index", "AppUser"); }

                if (Session["UniqueId"].ToString() != "" && Session["OrgInfoId"].ToString() != "" && Session["AppUserName"].ToString() != "")
                {
                    if (!new string[] { "AD", "SA" }.Contains(Convert.ToString(Session["SortCode"])))
                        return RedirectToAction("Index", "AppUser");
                }
                else { return RedirectToAction("Index", "AppUser"); }
            }
            catch (Exception) { return RedirectToAction("Index", "AppUser"); }
            return View();
        }

        [HttpGet]
        public JsonResult getData()
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var ObjData = (from sc2 in db.mAssetSubCategory2
                           join sc1 in db.mIteamTypeMasters on sc2.AssetSubCategoryId equals sc1.mIteamTypeMasterId
                           join gm in db.mGroupMasters on sc1.mGroupMasterId equals gm.mGroupMasterId
                           where sc2.IsActive == true
                           select new
                           {
                               sc2.AssetSubCategory2Id,
                               sc2.AssetSubCategory2Name,
                               sc2.AssetSubCategoryId,
                               SubCategoryName = sc1.IteamType,
                               AssetCategory = gm.GroupName,
                               sc2.Description,
                               sc2.IsActive
                           }).ToList();
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string SaveData(string _AssetSubCategory2Name, int _AssetSubCategoryId, string _Description)
        {
            var UserName = Session["AppUserName"];
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            mAssetSubCategory2 obj = new mAssetSubCategory2();
            string msg = "";
            try
            {
                if (db.mAssetSubCategory2.Any(o => o.AssetSubCategory2Name == _AssetSubCategory2Name && o.AssetSubCategoryId == _AssetSubCategoryId))
                    return msg = "Same Name Already Exist";
                else
                {
                    obj.AssetSubCategoryId = _AssetSubCategoryId;
                    obj.AssetSubCategory2Name = _AssetSubCategory2Name;
                    obj.Description = _Description;
                    obj.IsActive = true;
                    obj.CreatedDate = DateTime.Now; obj.CreatedBy = UserName.ToString();
                    obj.ModifiedDate = DateTime.Now; obj.ModifiedBy = UserName.ToString();
                    db.mAssetSubCategory2.Add(obj);
                    db.SaveChanges();
                    msg = "Data Saved";
                }
            }
            catch (Exception) { }
            return msg;
        }

        [HttpGet]
        public JsonResult getDataWithId(int ID)
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var datas = (from sc2 in db.mAssetSubCategory2
                         join sc1 in db.mIteamTypeMasters on sc2.AssetSubCategoryId equals sc1.mIteamTypeMasterId
                         join gm in db.mGroupMasters on sc1.mGroupMasterId equals gm.mGroupMasterId
                         where sc2.AssetSubCategory2Id == ID
                         select new
                         {
                             sc2.AssetSubCategory2Id,
                             sc2.AssetSubCategory2Name,
                             sc2.AssetSubCategoryId,
                             SubCategoryName = sc1.IteamType,
                             AssetCategory = gm.GroupName,
                             sc2.Description,
                             sc2.IsActive
                         }).ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public string UpdateData(string _AssetSubCategory2Name, int _AssetSubCategoryId, string _Description, int ID)
        {
            string msg = "";
            var UserName = Session["AppUserName"];
            var original = db.mAssetSubCategory2.FirstOrDefault(b => b.AssetSubCategory2Id == ID);
            if (original != null)
            {
                original.AssetSubCategory2Name = _AssetSubCategory2Name;
                original.AssetSubCategoryId = _AssetSubCategoryId;
                original.Description = _Description;
                original.ModifiedBy = UserName.ToString(); original.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                msg = "Data Updated";
            }
            else { return msg = "Data is Not updated"; }
            return msg;
        }

        [HttpGet]
        public string DeleteData(int ID)
        {
            string msg = "";
            mAssetSubCategory2 removeData = db.mAssetSubCategory2.Find(ID);
            if (removeData != null)
            {
                db.mAssetSubCategory2.Remove(removeData);
                db.SaveChanges();
                msg = "Deleted";
            }
            else { msg = "Unable to Deleted"; }
            return msg;
        }

        [HttpGet]
        public JsonResult getSubCategoryList()
        {
            var data = db.mIteamTypeMasters.Where(x => x.IsAction == true)
                .Select(c => new { c.mIteamTypeMasterId, c.IteamType }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getSubCategory1ByCategory(int categoryId)
        {
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var data = db.mIteamTypeMasters
                    .Where(x => x.mGroupMasterId == categoryId && x.IsAction == true)
                    .Select(c => new { c.mIteamTypeMasterId, c.IteamType })
                    .ToList();
                return Json(new { Flag = true, Message = "Data Loaded Successfully", DSubCategory1 = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Flag = false, Message = ex.Message, DSubCategory1 = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetBySubCategoryId(int subCategoryId)
        {
            var data = db.mAssetSubCategory2
                .Where(x => x.AssetSubCategoryId == subCategoryId && x.IsActive == true)
                .Select(c => new { c.AssetSubCategory2Id, c.AssetSubCategory2Name })
                .ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}

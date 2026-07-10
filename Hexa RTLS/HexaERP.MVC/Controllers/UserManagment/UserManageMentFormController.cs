using HexaERP.MVC.Models;
using HexaERP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{

    public class UserManageMentFormController : Controller
    {
        EncryptionDecryption Seq = new EncryptionDecryption();
        private ERPdbEntities HexaErpobj = new ERPdbEntities();

        // GET: UserManageMentForm
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

            //EncryptionDecryption jj = new EncryptionDecryption();
        }
        public JsonResult getOrganization()
        {
            //Mudassar I edited :30/01/2017
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = HexaErpobj.OrgInfoes
                                  .Where(b => b.OrgInfoId == orgId)
                                   .Select(p => new { p.OrgInfoId, p.OrgInfoName }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);

            //using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            //{
            //    var listoforganizations = HexaErpobj.OrgInfoes.ToList();
            //    return Json(listoforganizations, JsonRequestBehavior.AllowGet);
            //}
        }
        public JsonResult getDepartment()
        {
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                //int orgid = Convert.ToInt32(OrganizationID);
                //var listofdepartments = HexaErpobj.DepartMents.Where(b => b.OrgInfoId == orgid).ToList();
                var listofdepartments = HexaErpobj.DepartMents.ToList();
                return Json(listofdepartments, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getuserdata(string appuserid)
        {
            List<AppUserdd> collD = new List<AppUserdd>();
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            using (ERPdbEntities HexaErpobj = new ERPdbEntities())
            {
                int uid = Convert.ToInt32(appuserid);
                var listofusers = HexaErpobj.AppUsers.Where(b => b.AppUserId == uid && b.OrgInfoId == orgId).ToList();


                AppUserdd obj = new AppUserdd();
                foreach (var i in listofusers)
                {
                    obj.Password = Seq.Decrypt(i.Password);
                    obj.AppUserId = i.AppUserId;
                    obj.AppUserName = i.AppUserName;
                    obj.Mobile = i.Mobile;
                    obj.EMail = i.EMail;
                    obj.Sex = i.Sex;
                    obj.Address = i.Address;
                }
                collD.Add(obj);

                return Json(collD, JsonRequestBehavior.AllowGet);
            }
        }

        public string SaveUserData(string formdata)
        {
            string msg = "";
            try
            {
                var obj = JsonConvert.DeserializeObject<AppUser>(formdata);

                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                string _Password = Seq.Encrypt(obj.Password.Trim());

                using (ERPdbEntities HexaErpobj = new ERPdbEntities())
                {
                    AppUser _aObj = HexaErpobj.AppUsers.Where(x => x.AppUserName.Equals(obj.AppUserName) && x.Password.Equals(_Password) && x.IsAction == true).SingleOrDefault();

                    if (_aObj != null)
                    {
                        msg = "Please Choose differebt Credencial!";
                    }
                    else
                    {
                        //Mudassar i Edited:30/01/2017
                        int AppRoleId = 3;

                        var cFlag = HexaErpobj.Database.ExecuteSqlCommand(
                                  "INSERT INTO AppUser(EMail,AppUserName,AppRoleId,Password,DepartmentId,IsAction,CreatedDate,CreatedBy,OrgInfoId,IsAllowed,Sex,Mobile,Address)" +
                                  "VALUES('" + obj.EMail.Trim() + "','" + obj.AppUserName.Trim() + "','" + AppRoleId + "','" + _Password + "','" + obj.DepartmentId + "','" + true + "',GETDATE(),'" + UserName + "','" + orgId + "','" + true + "','" + obj.Sex + "','" + obj.Mobile + "','" + obj.Address + "')");
                        if (cFlag == 1)
                        {
                            msg = "Record Saved Successfully!";
                        }
                        HexaErpobj.SaveChanges();
                    }

                }

            }
            catch (DbEntityValidationException ex)
            {
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
            }
            return msg;
        }

        //27-12-2017 Mudassar
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(AppUser collection)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(collection.AppUserName) || string.IsNullOrEmpty(collection.Password) || collection.AppRoleId == null)
                {
                    _Flag = false; Message = "Fill mandetory fields";
                }
                else
                {
                    var Coutn = HexaErpobj.AppUsers.Count();

                    if (Convert.ToInt32(Coutn) > 20)
                    {
                        _Flag = false; Message = "Your no of excided please contact your service pervider";
                    }
                    else
                    {
                        if (HexaErpobj.AppUsers.Any(o => o.AppUserName == collection.AppUserName && o.OrgInfoId == orgId))
                        {
                            _Flag = false; Message = "Same Data Alerdy Exist";
                        }
                        else
                        {
                            string Uid = Guid.NewGuid().ToString();
                            string _Password = Seq.Encrypt(collection.Password.Trim());
                            collection.Password = _Password;
                            collection.UniqueId = Uid;
                            collection.IsAction = true; collection.IsAllowed = true;
                            collection.OrgInfoId = orgId;
                            collection.CreatedBy = UserName.ToString();
                            collection.CreatedDate = DateTime.Now;
                            HexaErpobj.AppUsers.Add(collection);
                            HexaErpobj.SaveChanges();
                            _Flag = true; Message = "User Added Successfully";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: UserManageMentForm/Delete/5
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
                    AppUser delObj = HexaErpobj.AppUsers.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        HexaErpobj.AppUsers.Remove(delObj);
                        HexaErpobj.SaveChanges();
                        _Flag = true; Message = "Record Deleted Successfully :" + delObj.AppUserName;
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        // GET: UserManageMentForm/Delete/5
        [HttpGet]
        public JsonResult AllowUser(int id, string Access)
        {
            bool _Flag = false; string Message = string.Empty;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (string.IsNullOrEmpty(Convert.ToString(id)))
                {
                    _Flag = false; Message = "Error While Allowing Access";
                }
                else
                {
                    AppUser delObj = HexaErpobj.AppUsers.Find(id);
                    if (delObj == null)
                    {
                        _Flag = false; Message = "Data Not Found";
                    }
                    else
                    {
                        if (Access == "false")
                        {
                            delObj.IsAllowed = true;
                            HexaErpobj.SaveChanges();
                            _Flag = true; Message = "Access Allowed Successfully :" + delObj.AppUserName;
                        }
                        else if (Access == "true")
                        {
                            delObj.IsAllowed = false;
                            HexaErpobj.SaveChanges();
                            _Flag = true; Message = "Access Denied  :" + delObj.AppUserName;
                        }
                        else { _Flag = false; Message = "Error While Updating Access"; }
                    }
                }
            }
            catch (Exception ex)
            {
                _Flag = false; Message = ex.Message;
            }
            return Json(new { Flag = _Flag, Message = Message }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public string UpdateUserData(string formdata, string APPUserID)
        {
            var obj = JsonConvert.DeserializeObject<AppUser>(formdata);
            string msg = "";
            using (ERPdbEntities HexaERPObj = new ERPdbEntities())
            {
                int userid = Convert.ToInt32(APPUserID);
                AppUser databaselist = HexaERPObj.AppUsers.Where(b => b.AppUserId == userid).FirstOrDefault();
                databaselist.AppUserName = obj.AppUserName;
                databaselist.DepartmentId = obj.DepartmentId;
                databaselist.Password = Seq.Encrypt(obj.Password);
                databaselist.Sex = obj.Sex;
                databaselist.Address = obj.Address;

                databaselist.EMail = obj.EMail;
                databaselist.Mobile = obj.Mobile;
                databaselist.ModifiedBy = Session["AppUserName"].ToString();
                databaselist.ModifiedDate = System.DateTime.Now;
                HexaERPObj.Entry(databaselist).State = EntityState.Modified;
                HexaERPObj.SaveChanges();
                //HexaERPObj.AppUsers.Add(databaselist);
                HexaERPObj.SaveChanges();
            }
            return msg;
        }
        public string DeleteUsersData(string appuserid)
        {
            string msg = "";
            using (ERPdbEntities HexaERPObj = new ERPdbEntities())
            {
                int uid = Convert.ToInt32(appuserid);
                HexaERPObj.AppUsers.RemoveRange(HexaERPObj.AppUsers.Where(b => b.AppUserId == uid));
                HexaERPObj.SaveChanges();
                msg = "Deleted successfully";
            }
            return msg;
        }
        public JsonResult BindUserDatatable()
        {
            List<AppUserdd> collD = new List<AppUserdd>();

            using (ERPdbEntities HexaERPObj = new ERPdbEntities())
            {

                //Mudassar I edited :30/01/2017
                //Get Organization Id From Session Variable
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var UsersData = (from p in HexaERPObj.AppUsers
                                 join c in HexaERPObj.OrgInfoes on p.OrgInfoId equals c.OrgInfoId
                                 join t in HexaERPObj.DepartMents on p.DepartmentId equals t.DepartMentID
                                 where (p.OrgInfoId == orgId && p.IsAction == true && p.IsAllowed == true && p.AppRoleId != 2 && p.AppRoleId != 1)
                                 select new { p.Password, AppUserId = p.AppUserId, AppUserName = p.AppUserName, OrgInfoName = c.OrgInfoName, DepartMentName = t.DepartMentName, Mobile = p.Mobile, EMail = p.EMail, Sex = p.Sex }).ToList();


                AppUserdd obj = new AppUserdd();
                foreach (var i in UsersData)
                {
                    obj.Password = Seq.Decrypt(i.Password);
                    obj.AppUserId = i.AppUserId;
                    obj.AppUserName = i.AppUserName;
                    obj.OrgInfoName = i.OrgInfoName;
                    obj.DepartMentName = i.DepartMentName;
                    obj.Mobile = i.Mobile;
                    obj.EMail = i.EMail;
                    obj.Sex = i.Sex;
                }
                collD.Add(obj);
                return Json(collD, JsonRequestBehavior.AllowGet);
            }

        }
        public partial class AppUserdd
        {
            public string OrgInfoName { get; set; }
            public string DepartMentName { get; set; }
            public int AppUserId { get; set; }
            public string EMail { get; set; }
            public string AppUserName { get; set; }
            public string Password { get; set; }
            public string Mobile { get; set; }
            public string Sex { get; set; }
            public string Address { get; set; }
        }

        [HttpGet]
        public JsonResult getMasterData()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                var Dept = HexaErpobj.DepartMents.ToList();
                var Desig = HexaErpobj.mDesignations.ToList();
                var _AppRoles = HexaErpobj.AppRoles.ToList();


                result = this.Json(new { Dept, Desig, _AppRoles }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
                result = this.Json(new { message }, JsonRequestBehavior.AllowGet);
                //_Flag = false; Message = message;
            }
            return result;
        }

        [HttpGet]
        public JsonResult getUserData()
        {
            // Initialization.    
            JsonResult result = new JsonResult();
            try
            {
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                List<getUserEnc> collD = new List<getUserEnc>();


                var Userd = (from us in HexaErpobj.AppUsers
                             join dept in HexaErpobj.DepartMents
                             on us.DepartmentId equals dept.DepartMentID into usdept
                             join des in HexaErpobj.mDesignations on us.DesignationId equals des.mDesignationId into usdes
                             join uty in HexaErpobj.AppRoles on us.AppRoleId equals uty.AppRoleId into usuty

                             where us.OrgInfoId == orgId
                             from usdeptd in usdept.DefaultIfEmpty()
                             from usdesd in usdes.DefaultIfEmpty()
                             from usutyd in usuty.DefaultIfEmpty()
                             select new
                             {
                                 us,
                                 usdeptd.DepartMentName,
                                 usdesd.Designation,
                                 // uty.AppRoleName,
                                 usutyd.SortCode
                             }
                       ).ToList();

                getUserEnc obj = new getUserEnc();

                foreach (var i in Userd)
                {
                    obj.Password = i.us.EMail;
                    obj.AppUserName = i.us.AppUserName;
                    obj.AppRoleId = i.us.AppRoleId;
                    obj.Theme = i.us.Theme;
                    obj.LanguageCode = i.us.LanguageCode;

                    obj.DepartmentId = i.us.DepartmentId;
                    obj.IsAction = i.us.IsAction;
                    obj.CreatedDate = i.us.CreatedDate;
                    obj.CreatedBy = i.us.CreatedBy;

                    obj.ModifiedDate = i.us.ModifiedDate;
                    obj.ModifiedBy = i.us.ModifiedBy;
                    obj.OrgInfoId = i.us.OrgInfoId;
                    obj.AccessLocation = i.us.AccessLocation;
                    obj.WorkingLocation = i.us.WorkingLocation;
                    obj.DesignationId = i.us.DesignationId;
                    obj.Mobile = i.us.Mobile;
                    obj.Sex = i.us.Sex;
                    obj.Address = i.us.Address;
                    obj.IsFirstTimeLoggedIn = i.us.IsFirstTimeLoggedIn;
                    obj.UniqueId = i.us.UniqueId;
                    obj.TodaysCallCount = i.us.TodaysCallCount;
                    obj.PwdRecoveryEmail = i.us.PwdRecoveryEmail;
                    obj.LoginFailedAttempt = i.us.LoginFailedAttempt;
                    obj.LoginFailedAttemptDuration = i.us.LoginFailedAttemptDuration;
                    obj.IsAllowed = i.us.IsAllowed;

                    obj.DepartMentName = i.DepartMentName;
                    obj.Designation = i.Designation;
                    obj.SortCode = i.SortCode;

                    obj.Password = Seq.Decrypt(i.us.Password);
                    collD.Add(obj);
                }


                result = this.Json(new { Userd }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                var message = string.Empty;
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    // Get entry
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;
                    // Display or log error messages
                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        message = string.Format("Error '{0}' occurred in {1} at {2}",
                                 subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        Console.WriteLine(message);
                    }
                }
                result = this.Json(new { message }, JsonRequestBehavior.AllowGet);
                //_Flag = false; Message = message;
            }
            return result;
        }
        //27-Dec-2017
        public partial class getUserEnc
        {

            public string DepartMentName { get; set; }
            public string Designation { get; set; }
            public string SortCode { get; set; }
            //
            public int AppUserId { get; set; }
            public string EMail { get; set; }
            public string AppUserName { get; set; }
            public string ReferenceId { get; set; }
            public Nullable<int> AppRoleId { get; set; }
            public string Password { get; set; }
            public string Theme { get; set; }
            public string LanguageCode { get; set; }
            public Nullable<int> DepartmentId { get; set; }
            public Nullable<bool> IsAction { get; set; }
            public Nullable<System.DateTime> CreatedDate { get; set; }
            public string CreatedBy { get; set; }
            public Nullable<System.DateTime> ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }
            public Nullable<int> OrgInfoId { get; set; }
            public string AccessLocation { get; set; }
            public Nullable<int> WorkingLocation { get; set; }
            public Nullable<int> DesignationId { get; set; }
            public string Mobile { get; set; }
            public string Sex { get; set; }
            public string Address { get; set; }
            public bool IsFirstTimeLoggedIn { get; set; }
            public string UniqueId { get; set; }
            public Nullable<short> TodaysCallCount { get; set; }
            public string PwdRecoveryEmail { get; set; }
            public Nullable<int> LoginFailedAttempt { get; set; }
            public Nullable<System.DateTime> LoginFailedAttemptDuration { get; set; }
            public bool IsAllowed { get; set; }
        }
    }
}
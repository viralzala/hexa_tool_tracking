using HexaERP.MVC.Models;
using HexaERP.Services;
using HexaPatch;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class AppUserController : Controller
    {
        //PrincipalContext adContext = new PrincipalContext(ContextType.Domain);
        private ERPdbEntities db = new ERPdbEntities();
        // GET: AppUser
        private const string SPLIT_1 = "\\";

        EncryptionDecryption Sec = new EncryptionDecryption();
        HexaPatchInclude lobj = new HexaPatchInclude();

        public ActionResult Index()
        {
            try
            {
                Session.Remove("AppUserName");
                Session.Remove("UniqueId");
                Session.Remove("OrgInfoId");
                Session.Remove("SortCode");
                Session.Contents.RemoveAll();
                //--- Get cookie Collection.
                HttpCookie cookieObj = Request.Cookies["HexaCookie"];
                if (cookieObj != null)
                {
                    //--- To delete cookie we will add negative time.
                    cookieObj.Expires = DateTime.Now.AddDays(-1);
                    //---- Add cookie to cookie collection.
                    Response.Cookies.Add(cookieObj);
                }

            }
            catch (Exception)
            {

            }
            return View();
        }

        public partial class _Platforms
        {
            public string Mac { get; set; }
        }
        //public List<GroupPrincipal> GetGroups(string userName)
        //{
        //    List<GroupPrincipal> result = new List<GroupPrincipal>();

        //    // establish domain context
        //    PrincipalContext yourDomain = new PrincipalContext(ContextType.Domain, "hexahash.com");

        //    // find your user
        //    UserPrincipal user = UserPrincipal.FindByIdentity(yourDomain, userName);

        //    // if found - grab its groups
        //    if (user != null)
        //    {
        //        PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups();

        //        // iterate over all groups
        //        foreach (Principal p in groups)
        //        {
        //            // make sure to add only group principals
        //            if (p is GroupPrincipal)
        //            {
        //                result.Add((GroupPrincipal)p);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AppUser Obj)
        {
            string _redir = string.Empty, _Domain = string.Empty, _UserName = string.Empty;

            if (ModelState.IsValid)
            {
                //string pcMac = Sec.MachineMac();
                //var mac = lobj.Platforms();
                //bool has = mac.Any(cus => cus.Mac == pcMac);

                //if (!has)
                //{
                //    ModelState.AddModelError("", "This hosted server are not licenced. please contact your service pervider");
                //    return View(Obj);
                //}


                if (Obj.IsAllowed == true)
                {
                    bool IsADuser = ValidateW(Obj.AppUserName, Obj.Password);
                    if (IsADuser == true)
                    {

                        try
                        {
                            if (Obj.AppUserName.IndexOf("\\") != -1)
                            {
                                string[] arrT = Obj.AppUserName.Split(SPLIT_1[0]);
                                _Domain = arrT[0];
                                _UserName = arrT[1];
                            }

                            var _pc = new PrincipalContext(ContextType.Domain, _Domain, _UserName, Obj.Password);
                            var _up = UserPrincipal.FindByIdentity(_pc, IdentityType.SamAccountName, _UserName);

                            foreach (var d in _up.GetGroups())
                            {
                                if (Convert.ToString(d) == "Hexa_SA")
                                {
                                    _redir = Convert.ToString(d);
                                    Session["AppUserName"] = _Domain + "/" + _up.DisplayName;
                                    Session["OrgInfoId"] = "1037";
                                    Session["SortCode"] = "SA";
                                    Session["UniqueId"] = DateTime.Now.ToString().GetHashCode().ToString("x");

                                    Session["adUser"] = Obj.AppUserName;
                                    Session["adpass"] = Obj.Password;
                                    //---- Method second
                                    //--- Create Cookie Object.
                                    HttpCookie cookieObject = new HttpCookie("HexaCookie");

                                    //--- Add values to cookie in Key,Value format.
                                    cookieObject["AppUserName"] = _Domain + "/" + _up.DisplayName;
                                    cookieObject["OrgInfoId"] = "1037";
                                    cookieObject["SortCode"] = "SA";
                                    cookieObject["UniqueId"] = DateTime.Now.ToString().GetHashCode().ToString("x");
                                    //---- Set expiry time of cookie.
                                    cookieObject.Expires.AddDays(1);
                                    //---- Add cookie to cookie collection.
                                    Response.Cookies.Add(cookieObject);

                                }
                                else if (Convert.ToString(d) == "Hexa_AD")
                                {
                                    _redir = Convert.ToString(d);
                                    Session["AppUserName"] = _Domain + "/" + _up.DisplayName;
                                    Session["OrgInfoId"] = "1037";
                                    Session["SortCode"] = "AD";
                                    Session["UniqueId"] = DateTime.Now.ToString().GetHashCode().ToString("x");

                                    Session["adUser"] = Obj.AppUserName;
                                    Session["adpass"] = Obj.Password;
                                    //---- Method second
                                    //--- Create Cookie Object.
                                    HttpCookie cookieObject = new HttpCookie("HexaCookie");

                                    //--- Add values to cookie in Key,Value format.
                                    cookieObject["AppUserName"] = _Domain + "/" + _up.DisplayName;
                                    cookieObject["OrgInfoId"] = "1037";
                                    cookieObject["SortCode"] = "AD";
                                    cookieObject["UniqueId"] = DateTime.Now.ToString().GetHashCode().ToString("x");
                                    //---- Set expiry time of cookie.
                                    cookieObject.Expires.AddDays(1);
                                    //---- Add cookie to cookie collection.
                                    Response.Cookies.Add(cookieObject);
                                }
                                else if (Convert.ToString(d) == "Hexa_OT")
                                {
                                    _redir = Convert.ToString(d);
                                    Session["AppUserName"] = _Domain + "/" + _up.DisplayName;
                                    Session["OrgInfoId"] = "1037";
                                    Session["SortCode"] = "OT";
                                    Session["UniqueId"] = DateTime.Now.ToString().GetHashCode().ToString("x");

                                    Session["adUser"] = Obj.AppUserName;
                                    Session["adpass"] = Obj.Password;
                                    //---- Method second
                                    //--- Create Cookie Object.
                                    HttpCookie cookieObject = new HttpCookie("HexaCookie");

                                    //--- Add values to cookie in Key,Value format.
                                    cookieObject["AppUserName"] = _Domain + "/" + _up.DisplayName;
                                    cookieObject["OrgInfoId"] = "1037";
                                    cookieObject["SortCode"] = "OT";
                                    cookieObject["UniqueId"] = DateTime.Now.ToString().GetHashCode().ToString("x");
                                    //---- Set expiry time of cookie.
                                    cookieObject.Expires.AddDays(1);
                                    //---- Add cookie to cookie collection.
                                    Response.Cookies.Add(cookieObject);
                                }
                            }

                            if (_redir.Trim() == "Hexa_SA")
                            {
                                return RedirectToAction("Index", "SuperAdmin");
                            }
                            else if (_redir.Trim() == "Hexa_AD")
                            {
                                return RedirectToAction("Index", "AdminMaster");
                            }
                            else if (_redir.Trim() == "Hexa_OT")
                            {
                                return RedirectToAction("Index", "AdminMaster");
                            }
                            else
                            {
                                ModelState.AddModelError("", "This user not mapped any hexa group.");
                                return View(Obj);
                            }
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("", "problem with domian or user name.");
                            return View(Obj);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "This user name or password entered is incorrect or not a part of application.");
                        return View(Obj);
                    }
                }
                else
                {
                    using (ERPdbEntities db = new ERPdbEntities())
                    {
                        try
                        {
                            string _Password = Sec.Encrypt(Obj.Password);
                            var obj = (from AU in db.AppUsers
                                       join AR in db.AppRoles on AU.AppRoleId equals AR.AppRoleId
                                       where (AU.AppUserName == Obj.AppUserName && AU.Password == _Password && AU.IsAllowed == true)

                                       select new
                                       {
                                           AppUserName = AU.AppUserName,
                                           UniqueId = AU.UniqueId,
                                           OrgInfoId = AU.OrgInfoId,
                                           SortCode = AR.SortCode,
                                           IsFirstTimeLoggedIn = AU.IsFirstTimeLoggedIn
                                       });
                            if (obj != null)
                            {
                                foreach (var objs in obj)
                                {
                                    Session["Message"] = "";
                                    Session["AppUserName"] = objs.AppUserName.ToString();
                                    Session["UniqueId"] = objs.UniqueId.ToString();
                                    Session["OrgInfoId"] = objs.OrgInfoId.ToString();
                                    Session["SortCode"] = objs.SortCode.ToString();

                                    //---- Method second
                                    //--- Create Cookie Object.
                                    HttpCookie cookieObject = new HttpCookie("HexaCookie");

                                    //--- Add values to cookie in Key,Value format.
                                    cookieObject["AppUserName"] = objs.AppUserName.ToString();
                                    cookieObject["UniqueId"] = objs.UniqueId.ToString();
                                    cookieObject["OrgInfoId"] = objs.OrgInfoId.ToString();
                                    cookieObject["SortCode"] = objs.SortCode.ToString();

                                    //---- Set expiry time of cookie.
                                    cookieObject.Expires.AddDays(1);

                                    //---- Add cookie to cookie collection.
                                    Response.Cookies.Add(cookieObject);

                                    ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
                                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Obj.AppUserName));
                                    identity.AddClaim(new Claim(ClaimTypes.Role, objs.SortCode));

                                    AuthenticationManager.SignIn(new AuthenticationProperties()
                                    {
                                        AllowRefresh = true,
                                        IsPersistent = false,
                                        ExpiresUtc = DateTime.UtcNow.AddDays(1)
                                    }, identity);
                                    return RedirectToAction("Index", "EnterpriseAnalyticsDashboard");

                                    //if (objs.SortCode == "SA")
                                    //{
                                    //    return RedirectToAction("Index", "SuperAdmin");
                                    //}
                                    //else if (objs.SortCode == "AD")
                                    //{
                                    //    return RedirectToAction("Index", "AdminMaster");
                                    //}
                                    //else if (objs.SortCode == "OT")
                                    //{
                                    //    return RedirectToAction("Index", "AdminMaster");
                                    //    //return RedirectToAction("Index", "UserHome");
                                    //}
                                    //else if (objs.SortCode == "NAP")
                                    //{
                                    //    return RedirectToAction("Index", "ReturnApproveSMTBin");
                                    //    //return RedirectToAction("Index", "UserHome");
                                    //}
                                    //else if (objs.SortCode == "QAP")
                                    //{
                                    //    return RedirectToAction("Index", "QualityReturnApproveSMTBin");
                                    //    //return RedirectToAction("Index", "UserHome");
                                    //}
                                    //else
                                    //{

                                    //}
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "The user name or password entered is incorrect.");
                                return View(Obj);
                            }
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", ex.Message.ToString());
                            return View(Obj);
                        }
                    }
                }
            }
            // ModelState.AddModelError("", "The user name or password entered is incorrect.");
            return View(Obj);
        }

        public void IdentitySingout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
        }
        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }
        public static bool ValidateW(string UserName, string Password)
        {
            bool valid = false;
            string Domain = "";
            try
            {
                if (UserName.IndexOf("\\") != -1)
                {
                    string[] arrT = UserName.Split(SPLIT_1[0]);
                    Domain = arrT[0];
                    UserName = arrT[1];
                }

                if (Domain.Length == 0)
                {
                    Domain = System.Environment.MachineName;
                }

                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, Domain))
                {
                    valid = context.ValidateCredentials(UserName, Password);
                }
            }
            catch (Exception)
            {
                valid = false;
            }
            return valid;
        }

        //
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ChangePassword(FormCollection Obj)
        {
            // Initialization.    
            // JsonResult result = new JsonResult();

            string _Password = Sec.Encrypt(Obj["Password"]);
            string _AppUserName = Obj["AppUserName"];
            string _PwdRecoveryEmail = Sec.Encrypt(Obj["PwdRecoveryEmail"]);
            var UserName = Session["AppUserName"];

            //int orginf = Convert.ToInt32(Session["OrgInfoId"]);

            using (ERPdbEntities db = new ERPdbEntities())
            {
                try
                {
                    var obj = (from AU in db.AppUsers
                               where (AU.AppUserName == _AppUserName && AU.Password == _Password && AU.IsAllowed == true)
                               select new
                               {
                                   AU.AppUserId,
                                   AU.AppUserName,
                                   AU.UniqueId,
                                   AU.OrgInfoId,
                                   AU.IsFirstTimeLoggedIn

                               }).SingleOrDefault();

                    if (obj != null)
                    {
                        AppUser EditObj = db.AppUsers.Find(obj.AppUserId);
                        EditObj.Password = _PwdRecoveryEmail;
                        EditObj.ModifiedBy = UserName.ToString();
                        EditObj.ModifiedDate = DateTime.Now;
                        db.Entry(EditObj).State = EntityState.Modified;
                        db.SaveChanges();

                        Session["message"] = "New password created for your account successfully"; Session["Flag"] = true;

                        //result = this.Json(new { message = "Activity Updated Successfully", Flag = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["message"] = "The user name or password entered is incorrect"; Session["Flag"] = false;
                        // result = this.Json(new { message = "The user name or password entered is incorrect", Flag = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    Session["message"] = ex.InnerException.Message.ToString(); Session["Flag"] = false;
                    //result = this.Json(new { message = ex.InnerException.Message.ToString(), Flag = false }, JsonRequestBehavior.AllowGet);
                }
            }

            return RedirectToAction("Index", "RedirectNotification");
            //return Response.RedirectToRoutePermanent("","");

        }
        //
        [HttpGet]
        public string Logout()
        {
            try
            {
                //Removing Session
                Session.Abandon();
                Session.Clear();
                Session.RemoveAll();

                Session.Remove("AppUserName"); Session.Remove("UniqueId");
                Session.Remove("OrgInfoId"); Session.Remove("SortCode");
                Session.Contents.RemoveAll();

                //--- Get cookie Collection.
                HttpCookie cookieObj = Request.Cookies["HexaCookie"];

                //--- To delete cookie we will add negative time.
                cookieObj.Expires = DateTime.Now.AddDays(-1);

                //---- Add cookie to cookie collection.
                Response.Cookies.Add(cookieObj);

                new AppUserController().IdentitySingout();

            }
            catch (Exception)
            {
            }
            return ("../AppUser/Index");
        }
    }
}
using HexaERP.MVC.Models;
using HexaERP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace HexaERP.MVC.Controllers
{

    public class ProfileController : ApiController
    {
        private ERPdbEntities db = new ERPdbEntities();
        EncryptionDecryption Sec = new EncryptionDecryption();

        public class LoginModelView
        {
            public string Username { get; set; }
            public string Password { get; set; }

        }


        //[Authorize]
        [HttpGet]
        //[Route("api/Profile")]
        public async Task<HttpResponseMessage> GetProfilestrsssa(string email)
        {
            try
            {
                //if (User is ClaimsPrincipal)
                //{
                //    var user = User as ClaimsPrincipal;
                //    var details = user.Claims.ToList();
                //    var Userid = authenticationDetails(details, ClaimTypes.NameIdentifier);
                //    var Name = authenticationDetails(details, ClaimTypes.Name);
                //    var Email = authenticationDetails(details, ClaimTypes.Email);
                //    var Role = authenticationDetails(details, ClaimTypes.Role);
                //    var plant = authenticationDetails(details, "plant");
                //    var organization = authenticationDetails(details, "organization");

                //    var content = new { status = 1, message = "Successfully", content = new { Name, Userid, Email, plant, organization, Role } };
                //    return new HttpResponseMessage
                //    {
                //        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                //    };

                //}
                //else
                //{
                //    var content = new { status = 0, message = "Record Not Found" };
                //    return new HttpResponseMessage
                //    {
                //        Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                //    };
                //}

                //var obj = await (from AU in db.AppUsers
                //                 where (AU.em == appUser.Username && AU.Password == _Password && AU.IsAllowed == true)
                //                 select new
                //                 {
                //                     AU.em
                //                 });


                var content = new { status = 0 };
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                var content = new { status = 0, message = ex.Message };
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<HttpResponseMessage> Login([FromBody] LoginModelView appUser)
        {
            if (appUser == null)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { status = false, message = "Parameter is null" }), System.Text.Encoding.UTF8, "application/json")
                };
            }
            string _Password = Sec.Encrypt(appUser.Password);

            var obj = await (from AU in db.AppUsers
                             join AR in db.AppRoles on AU.AppRoleId equals AR.AppRoleId into _role
                             join em in db.tEmployeeTags on AU.AppUserName equals em.EmailId into _emp
                             where (AU.AppUserName == appUser.Username && AU.Password == _Password && AU.IsAllowed == true)
                             from role in _role.DefaultIfEmpty()
                             from emp in _emp.DefaultIfEmpty()
                             select new
                             {
                                 EmployeeID = emp.tEmployeeTagId,
                                 // FullName = string.Format($"{AU.}"),
                                 AppUserName = AU.AppUserName,
                                 UniqueId = AU.UniqueId,
                                 OrgInfoId = AU.OrgInfoId,
                                 SortCode = role.SortCode,
                                 IsFirstTimeLoggedIn = AU.IsFirstTimeLoggedIn
                             }).FirstOrDefaultAsync();

            if (obj != null)
            {

                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { status = true, message = "Sucess", data = obj }), System.Text.Encoding.UTF8, "application/json")
                };

            }
            else
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { status = false, message = "Invalid Username or Password" }), System.Text.Encoding.UTF8, "application/json")
                };
            }
            //string[] userRle = { "" };
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            //string[] userRoles = { "" };
            //ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ""));
            //identity.AddClaim(new Claim(ClaimTypes.Email, ""));
            //userRoles.ToList().ForEach((role) => identity.AddClaim(new Claim(ClaimTypes.Role, role)));
            //identity.AddClaim(new Claim(ClaimTypes.Name, ""));
            //AuthenticationManager.SignIn(new AuthenticationProperties()
            //{
            //    AllowRefresh = true,
            //    IsPersistent = false,
            //    ExpiresUtc = DateTime.UtcNow.AddDays(1)
            //}, identity);
        }

        // private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
        public static string authenticationDetails(List<Claim> value, string key)
        {
            var details = value.FirstOrDefault(c => c.Type == key);
            if (details == null)
                return null;

            return details.Value;


        }
    }
}

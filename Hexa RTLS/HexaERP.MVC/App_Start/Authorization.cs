using HexaERP.MVC.Repository;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HexaERP.MVC.App_Start
{
    public class ApplicationAuthorization : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (var _repo = new AppicationUser())
            {
                var user = _repo.ValidateUser(context.UserName, context.Password, context.ClientId);
                if (user == null)
                {
                    context.SetError("invalid_grant", "Provided username and password is incorrect");
                    context.Rejected();
                    return;
                }

                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.userId));
                foreach (var item in user.role) { identity.AddClaim(new Claim(ClaimTypes.Role, item)); }
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
                identity.AddClaim(new Claim(ClaimTypes.Email, user.email));
                identity.AddClaim(new Claim("WorkingLocation", Convert.ToString(user.WorkingLocation)));
                identity.AddClaim(new Claim("organization", Convert.ToString(user.organization)));
                context.Validated(identity);
            }
        }
    }
}
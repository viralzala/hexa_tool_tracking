using Hangfire;
using Hangfire.SqlServer;
using HexaERP.MVC.Jobs;
using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web.Helpers;

[assembly: OwinStartup(typeof(HexaERP.MVC.Startup))]
namespace HexaERP.MVC
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {

            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            //{
            //    AllowInsecureHttp = true,
            //    TokenEndpointPath = new PathString("/token"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromDays(30),
            //    Provider = new ApplicationAuthorization()
            //};

            //app.UseOAuthAuthorizationServer(options);
            //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            //HttpConfiguration config = new HttpConfiguration();
            //WebApiConfig.Register(config);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/AppUser/Index"),
                LogoutPath = new PathString("/AppUser/Logout")
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            GlobalConfiguration.Configuration
                .UseSqlServerStorage("HexaRTLSEventsConn", new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(1) })
                .UseFilter(new LogFailureAttribute());

            try
            {
                using (var contx = new ERPdbEntities())
                {
                    if (contx.mReaderSettups.Count() < 21)
                    {
                        RecurringJob.AddOrUpdate(() => new TriggerTracking().MasterTrigger(), Cron.MinuteInterval(2));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().BinNotOrReachesAssembly(), Cron.MinuteInterval(2));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().UnknownGateway(), Cron.MinuteInterval(2));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().ReturnBIN(), Cron.MinuteInterval(5));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().QualityReturn(), Cron.MinuteInterval(5));

                    }
                    else
                    {
                        RecurringJob.AddOrUpdate(() => new TriggerTracking().MasterTrigger(), Cron.Yearly(2));

                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().BinNotOrReachesAssembly(), Cron.Yearly(2));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().UnknownGateway(), Cron.Yearly(2));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().ReturnBIN(), Cron.Yearly(5));
                        //RecurringJob.AddOrUpdate(() => new TriggerTracking().QualityReturn(), Cron.Yearly(5));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
            }



            app.UseHangfireDashboard();
            app.UseHangfireServer();
            app.MapSignalR();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;


            //ConfigureAuth(app);
        }



        public void ConfigureAuth(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

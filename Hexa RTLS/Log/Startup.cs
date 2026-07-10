using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;

namespace Log
{
    public class Startup
    {
        public Startup()
        {
            string url = "http://localhost:54828";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
        public class ReaderStatusHub1 : Hub
        {
            //public void Send(string name, string message)
            //{
            //    Clients.All.addMessage(name, message);
            //}
        }
    }
}

using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class RedirectNotificationController : Controller
    {
        // GET: RedirectNotification
        public ActionResult Index()
        {
            ViewBag.message = Session["message"].ToString();
            ViewBag.Flag = Session["Flag"];
            return View();
        }
    }
}
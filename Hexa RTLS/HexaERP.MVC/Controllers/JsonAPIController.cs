using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers
{
    public class JsonAPIController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        //private static IHubContext _APIStatusHub = GlobalHost.ConnectionManager.GetHubContext<APIStatusHub>();
        //
        static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();
        static Dictionary<string, int> tagsRead = new Dictionary<string, int>();
        JsonResult _result = new JsonResult();
        // GET: JsonAPI
        private static double RssiVal =
            Convert.ToDouble(WebConfigurationManager.AppSettings["RssiVal"]);
        public ActionResult Index()
        {
            return View();
        }

        // GET: JsonAPI/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: JsonAPI/Create
        public ActionResult Create()
        {
            return View();
        }


        //public static void Callback(object state)
        //{
        //    if (tagsRead.Count > 0)
        //    {
        //        tagsRead.Clear();
        //        tagsTrack.Clear();
        //        _APIStatusHub.Clients.All.getAPIStatus(DateTime.Now.ToShortTimeString(), true);
        //    }
        //    else { _APIStatusHub.Clients.All.getAPIStatus("Data Not Filttering", false); }


        //}

        // POST: JsonAPI/Create
        [HttpPost]
        [Route("api/TrackLog")]
        public ActionResult Post(List<toTrackInfo> formdata)
        {
            try
            {
                if (formdata.Count > 0)
                {
                    formdata.ToList().ForEach(
       obj =>
       {
           String EpcKey;
           EpcKey = obj.RFID.ToString() + "-" + obj.ReaderNo + "-" + obj.mAttPortId.ToString();
           //double Rssi = Math.Abs(RssiVal);
           if (!tagsRead.ContainsKey(EpcKey))
           {
               // Add this tag to the list of tags we've read.
               // InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber, sender.Name, sender.Address);
               tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
               db.toTrackInfoes.Add(obj);
               db.SaveChanges();

               //if (Rssi <= RssiVal)
               //{

               //}
           }

       });
                    //String Key; string PortId;

                }
                else
                {
                    _result = Json(new { Flag = false, Message = "Empty Form" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                _result = Json(new { Flag = false, ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return _result;
        }

        // GET: JsonAPI/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: JsonAPI/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: JsonAPI/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: JsonAPI/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

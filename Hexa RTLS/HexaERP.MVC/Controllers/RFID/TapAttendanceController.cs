using J_RFID;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class TapAttendanceController : Controller
    {
        RFIDAPI NFC_API = new RFIDAPI();
        int Err = 0;

        string[] UID_ID = new string[500];
        ulong[] UID_Count = new ulong[500];

        //string EPC = "", Cardtype = "0";
        //ulong EPC_Count = 0;
        //int Tim4out = 0;

        static string outstr = "", test = "", Times = "100";
        //static string Str = ""; static bool Flag = false;
        static Dictionary<string, DateTime> TagObj = new Dictionary<string, DateTime>();

        static List<GetReaderObj> TagColl = new List<GetReaderObj>();

        // GET: TapAttendance
        public ActionResult Index()
        {
            TagObj.Clear();
            TagColl.Clear();
            return View();
        }

        // GET: 
        [HttpGet]
        public JsonResult GetPorts()
        {
            string[] strAryCom = null;

            Dictionary<string, int> dObj = new Dictionary<string, int>();

            strAryCom = SerialPort.GetPortNames();

            for (int i = 0; i < strAryCom.Length; i++)
            {
                dObj.Add(strAryCom[i], i);
                //ComBxTm_COM2.Items.Add((object)strAryCom[i]);
            }
            return Json(dObj.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: 
        [HttpGet]
        public JsonResult PutStart(string _Port)
        {
            string Str = ""; //bool Flag = false;

            if (_Port == "" || _Port == null)
            {
                return Json(new { Flag = false, Msg = "Port Null" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                Err = NFC_API.UHC_OpenReader(_Port);
                if (Err != 0)
                {
                    Str = ("Open COM Err " + Err);
                }
                Err = NFC_API.UHF_FwVersion(out Str);
                Err = NFC_API.UHF_ReaderID(out Str);
                return Json(new { Flag = true, Msg = "Reader Started Tap the Tag" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                NFC_API.UHF_CloseReader(_Port);
                Str = "Error In Port:" + ex;
                return Json(new { Flag = false, Msg = Str }, JsonRequestBehavior.AllowGet);
            }

            //return Json(new { Flag = Flag, Msg = Str }, JsonRequestBehavior.AllowGet);
        }

        // GET: 
        [HttpGet]
        public JsonResult getData()
        {

            try
            {
                Err = NFC_API.UHF_GetEPC(Times, out outstr);

                if (Err != 1)
                {
                    test = Regex.Replace(outstr, ".{4}", "$0 ").Trim();

                    if (!TagObj.ContainsKey(test))
                    {
                        GetReaderObj obj = new GetReaderObj();
                        obj.RFID = test; obj.tDate = DateTime.Now;
                        // Add this tag to the list of tags we've read.                       
                        TagObj.Add(test, DateTime.Now);
                        TagColl.Add(obj);
                    }
                    return Json(new { Flag = true, Msg = "New Tag", Datas = TagColl }, JsonRequestBehavior.AllowGet);
                    //Flag = true; Str = "New Tag";
                }
                //else {
                //    //return Json(new { Flag = false, Msg = "Error Failed to connect the reader" }, JsonRequestBehavior.AllowGet);
                //}

            }
            catch (Exception ex) { return Json(new { Flag = false, Msg = "Error:" + ex }, JsonRequestBehavior.AllowGet); }

            return Json(new { Flag = false, Msg = "Reader Ideal" }, JsonRequestBehavior.AllowGet);
            //return Json(TagColl, JsonRequestBehavior.AllowGet);
        }

        public partial class GetReaderObj
        {
            public string RFID { get; set; }
            public DateTime tDate { get; set; }
        }
    }
}
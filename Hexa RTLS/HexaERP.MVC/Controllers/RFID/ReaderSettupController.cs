using HexaERP.MVC.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class ReaderSettupController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 24/02/2017
        //FloorMasterController
        //******************
        //EncryptionDecryption Sec = new EncryptionDecryption();
        private ERPdbEntities db = new ERPdbEntities();

        // GET: ReaderSettup
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
        }

        [HttpGet]
        public JsonResult GetRoomsData()
        {
            //Get Organization Id From Session Variable
            int _orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from ara in db.mRoomMasters
                           join st in db.mSiteMasters on ara.mSiteMasterId equals st.mSiteMasterId into stara
                           join zn in db.mZones on ara.mZoneId equals zn.mZoneId into znara
                           join sz in db.mFloorMasters on ara.mFloorMasterId equals sz.mFloorMasterId into szara
                           where (ara.OrgInfoId == _orgId)
                           from Site in stara.DefaultIfEmpty()
                           from Zone in znara.DefaultIfEmpty()
                           from SubZone in szara.DefaultIfEmpty()
                           select new
                           {
                               ara.mRoomMasterId,
                               Site.Site,
                               Zone.Zone,
                               SubZone.FloorName,
                               ara.RoomName,
                           }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetReadersSettup(int Rid)
        {
            //Get Organization Id From Session Variable
            int _orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from mRd in db.mReaderSettups
                           join mRm in db.mRoomMasters on mRd.mRoomMasterId equals mRm.mRoomMasterId into CUT_RMS
                           where (mRd.OrgInfoId == _orgId && mRd.mRoomMasterId == Rid)
                           from RdData in CUT_RMS.DefaultIfEmpty()
                           select new
                           {
                               mRd.mReaderSettupId,
                               mRd.mRoomMasterId,
                               mRd.ReaderNo,
                               mRd.ReaderIP,
                               mRd.AttPortId,
                               RdData.RoomName,
                               RdData.RoomNo,
                               mRd.lat,
                               mRd.lng,
                               mRd.description
                           }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult putReaderData(mReaderSettup mObj)
        {
            //Get Organization Id From Session Variable
            int _orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            if (mObj.mRoomMasterId == null || mObj.ReaderNo == null || mObj.AttPortId == null)
            {
                return Json(new { result = false, Message = "Please fill all the fileds ", Url = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (db.mReaderSettups.Any(x => x.ReaderNo == mObj.ReaderNo && x.AttPortId == mObj.AttPortId && x.IsAction == true && x.OrgInfoId == _orgId))
                {
                    return Json(new { result = false, Message = "Already same reader and port assined", Url = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    mRoomMaster mRoomMaster = db.mRoomMasters.Find(mObj.mRoomMasterId);

                    if (mRoomMaster != null)
                    {
                        mObj.mSiteMasterId = mRoomMaster.mSiteMasterId;
                        mObj.mZoneId = mRoomMaster.mZoneId;
                        mObj.mFloorMasterId = mRoomMaster.mFloorMasterId;
                        mObj.CreatedBy = "";
                        mObj.CreatedDate = DateTime.Now;
                        mObj.OrgInfoId = _orgId; mObj.IsAction = true;
                        db.mReaderSettups.Add(mObj);
                        db.SaveChangesAsync();
                        return Json(new { result = true, Message = "Data Saves Successfully", Url = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else { return Json(new { result = false, Message = "Data Missmatch", Url = "" }, JsonRequestBehavior.AllowGet); }


                }
            }
        }

        [HttpPost]
        public JsonResult updateReaderData(mReaderSettup mObj)
        {
            //Get Organization Id From Session Variable
            int _orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            if (mObj.mRoomMasterId == null || mObj.ReaderNo == null || mObj.AttPortId == null)
            {
                return Json(new { result = false, Message = "Please fill all the fileds ", Url = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (db.mReaderSettups.Any(x => x.ReaderNo == mObj.ReaderNo && x.AttPortId == mObj.AttPortId && x.IsAction == true && x.OrgInfoId == _orgId))
                {
                    return Json(new { result = false, Message = "Already same reader and port assined or you are updating same data", Url = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var original = db.mReaderSettups.FirstOrDefault(b => b.mReaderSettupId == mObj.mReaderSettupId);

                    if (original != null)
                    {
                        original.lat = mObj.lat;
                        original.lng = mObj.lng;
                        original.description = mObj.description;
                        //original.title = mObj.ReaderNo;

                        original.ReaderNo = mObj.ReaderNo;
                        original.ReaderIP = mObj.ReaderIP;
                        original.AttPortId = mObj.AttPortId;
                        original.ModifiedDate = DateTime.Now;
                        original.IsAction = true;
                        original.ModifiedBy = "";

                        db.SaveChanges();
                        return Json(new { result = true, Message = "Data Updated Successfully!", Url = "" }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new { result = false, Message = "Data Not Found", Url = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        //
        [HttpGet]
        public JsonResult DeleteReaderData(int ID)
        {
            //Get Organization Id From Session Variable
            int _orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            mReaderSettup removeData = db.mReaderSettups.Find(ID);
            if (removeData != null)
            {
                db.mReaderSettups.Remove(removeData);
                db.SaveChanges();
                return Json(new { result = true, Message = " Data Deleted", Url = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = false, Message = "Unable to Deleted", Url = "" }, JsonRequestBehavior.AllowGet);
            }
            //Convert List Data to The Json Array          

        }
        //
        [HttpGet]
        public JsonResult getReaderMac(string Ipaddress)
        {
            string MAC = "";
            try
            {
                MAC = GetClientMac(Ipaddress);
                if (MAC != null && MAC != "")
                {
                    return Json(new { result = true, Message = "Mac:", Url = "", IData = MAC }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { result = false, Message = "Data Not Found", Url = "" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception) { return Json(new { result = false, Message = "Data Not Found", Url = "" }, JsonRequestBehavior.AllowGet); }
        }
        //
        private String GetClientMac(string IPAddress)
        {
            /* There is a good chance that the server does not have an "arp" record for a given IP address, */
            /* so to create an arp entry we first send a ping to the IP Address*/
            string mac = string.Empty;

            try
            {
                //Sending ping:
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "ping";
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = false;
                psi.Arguments = IPAddress;
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);
                process.WaitForExit();

                //Now run arp command:
                psi = new ProcessStartInfo();
                psi.FileName = "arp";
                psi.CreateNoWindow = false;
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = "-a " + IPAddress;
                psi.UseShellExecute = false;

                process = Process.Start(psi);
                string arpInfo = string.Empty;
                while (!process.StandardOutput.EndOfStream)
                {
                    arpInfo += process.StandardOutput.ReadLine();
                }

                process.WaitForExit();

                //Remove all white space from ARP result:
                arpInfo = arpInfo.Replace(" ", "");
                //Strip MAC from ARP result
                mac = arpInfo.Substring((arpInfo.IndexOf(IPAddress) + IPAddress.Length), 17);
            }
            catch (Exception)
            {
                //Error code here....
            }

            return mac.ToUpper();
        }
    }
}
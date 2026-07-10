using HexaERP.MVC.Models;
using HexaERP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class ReaderConfigController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 24/02/2017
        //FloorMasterController
        //******************
        EncryptionDecryption Sec = new EncryptionDecryption();
        private ERPdbEntities db = new ERPdbEntities();
        // GET: ReaderConfig
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetOrganizations()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.OrgInfoes.ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFloors()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.OrgInfoes.ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult getLoadData()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_LeadType.Where(o => o.OrgInfoId == orgId).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Get All Department
        [HttpGet]
        public JsonResult getData(int _orgId)
        {
            List<CustomeEnt> eList = new List<CustomeEnt>();
            try
            {
                var UserName = Session["AppUserName"];
                //Get Organization Id From Session Variable
                int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                //Get Selected Data Accourding to Org Id
                var ObjData = (from mRM in db.mRoomMasters
                               join mFloorM in db.mFloorMasters on mRM.mFloorMasterId equals mFloorM.mFloorMasterId into CUT_FLR
                               join OrgInfo in db.OrgInfoes on _orgId equals OrgInfo.OrgInfoId into CUT_LED
                               join mReadero in db.mReaders on mRM.mRoomMasterId equals mReadero.mRoomMasterId into CUT_LTY
                               where (mRM.OrgInfoId == _orgId)
                               from mFloorM in CUT_FLR.DefaultIfEmpty()
                               from OrgInfo in CUT_LED.DefaultIfEmpty()
                               from mReadero in CUT_LTY.DefaultIfEmpty()
                               select new { OrgInfo.OrgInfoId, OrgInfo.OrgInfoName, mRM.mRoomMasterId, mRM.RoomName, mRM.RoomNo, mFloorM.FloorName, mFloorM.FloorNo, mFloorM.mFloorMasterId, mReadero.ReaderIP, mReadero.mAttPortId, mReadero.Vfd, mReadero.Vtd, mReadero.mReaderId, mReadero.ReaderNo }).ToList();
                //Convert List Data to The Json Array       
                //Convert List Data to The Json Array     
                //<IList> ob = new IList();

                CustomeEnt obj = new CustomeEnt();

                foreach (var item in ObjData)
                {
                    eList.Add(new CustomeEnt
                    {
                        mReaderId = item.mReaderId,
                        OrgInfoId = item.OrgInfoId,
                        OrgInfoName = item.OrgInfoName,
                        mRoomMasterId = item.mRoomMasterId,

                        ReaderIP = item.ReaderIP,
                        mAttPortId = item.mAttPortId,

                        FloorName = item.FloorName,
                        FloorNo = item.FloorNo,
                        mFloorMasterId = item.mFloorMasterId,
                        RoomName = item.RoomName,
                        RoomNo = item.RoomNo,

                        ReaderNo = Sec.Decrypt(item.ReaderNo),
                        Vfd = Sec.Decrypt(item.Vfd),
                        Vtd = Sec.Decrypt(item.Vtd)
                    });
                }
            }
            //catch (DbEntityValidationException ex)
            //{
            //   string  msg = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
            //    throw new DbEntityValidationException(msg);
            //}
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
            return Json(eList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// RFIDs info
        /// </summary>
        public class CustomeEnt
        {
            public int? mReaderId;
            public int OrgInfoId;
            public string OrgInfoName;

            public string FloorName;
            public int? FloorNo;
            public int? mRoomMasterId;
            public string RoomName;
            public int? RoomNo;
            public int? mAttPortId;
            public int? mFloorMasterId;
            public String ReaderIP;
            public string ReaderNo;
            public String Vfd;
            public String Vtd;
        }

        //Save New Departmnt
        [HttpGet]
        public string SaveData(string formData)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //   mReader obj = new mReader();
            string msg = "";
            var objd = JsonConvert.DeserializeObject<mReader>(formData);
            var original = db.mReaders.FirstOrDefault(b => b.mReaderId == objd.mReaderId);
            if (original != null)
            {
                //obj.OrgInfoId = OrgInfoId;
                //obj.mFloorMasterId = mFloorMasterId;
                original.ReaderNo = Sec.Encrypt(objd.ReaderNo);
                original.Vfd = Sec.Encrypt(objd.Vfd);
                original.Vtd = Sec.Encrypt(objd.Vtd);
                original.ReaderIP = objd.ReaderIP;
                original.mAttPortId = objd.mAttPortId;
                original.ModifiedDate = DateTime.Now; original.ModifiedBy = UserName.ToString(); original.IsAction = true;
                db.SaveChanges();
                msg = "Data Saved";
            }
            else
            {
                return msg = "Data is Not Intalled";
            }

            return msg;
        }
        //Get Dept By Id
        [HttpGet]
        public JsonResult getDataWithId(int ID)
        {
            var datas = db.mReaders.Where(o => o.mReaderId == ID).ToList();
            List<CustomeEnt> eList = new List<CustomeEnt>();
            CustomeEnt obj = new CustomeEnt();
            foreach (var item in datas)
            {
                eList.Add(new CustomeEnt
                {
                    mReaderId = item.mReaderId,
                    ReaderIP = item.ReaderIP,
                    mAttPortId = item.mAttPortId,
                    Vfd = Sec.Decrypt(item.Vfd),
                    Vtd = Sec.Decrypt(item.Vtd),
                    ReaderNo = Sec.Decrypt(item.ReaderNo)
                });
            }
            return Json(eList, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateData(int mReaderId, string ReaderNo, string Vfd, string Vtd, string ReaderIP, int mAttPortId)
        {
            string msg = "";
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.mReaders.FirstOrDefault(b => b.mReaderId == mReaderId);
            if (original != null)
            {
                original.ReaderIP = ReaderIP;
                original.mAttPortId = mAttPortId;
                original.ReaderNo = Sec.Encrypt(ReaderNo);
                original.Vfd = Sec.Encrypt(Vfd);
                original.Vtd = Sec.Encrypt(Vtd);
                original.ModifiedBy = UserName.ToString();
                original.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                msg = "Data Updated";
            }
            else
            {
                return msg = "Data is Not updated";
            }
            return msg;
        }
        //Delete Department
        [HttpGet]
        public string DeleteData(int ID)
        {
            string msg = "";
            mReader removeData = db.mReaders.Find(ID);
            if (removeData != null)
            {
                //db.mReaders.Remove(removeData);
                //db.SaveChanges();
                //msg = "Deleted";
            }
            else
            {
                msg = "Unable to Deleted";
            }
            return msg;
        }
        [HttpGet]
        public string getReaderMac(string Ipaddress)
        {
            string MAC = "";
            try
            {
                MAC = GetClientMac(Ipaddress);
            }
            catch (Exception) { }
            return MAC;
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
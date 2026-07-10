using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class InquiryTypeMasterController : Controller
    {
        //******************
        //Author: Mudassar I
        //Date: 06/02/2017
        //Department Master
        //******************

        private ERPdbEntities db = new ERPdbEntities();
        // GET: LeadDispositionMaster
        public ActionResult Index()
        {
            return View();
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
        public JsonResult getData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.TC_InquiryType
                           join Inq in db.TC_LeadType on Dis.TC_LeadTypeId equals Inq.TC_LeadTypeId
                           where (Dis.OrgInfoId == orgId && Dis.IsActive == true)
                           select new { Dis.TC_InquiryTypeId, Dis.InquiryType, Inq.LeadType }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Save New Departmnt
        [HttpGet]
        public string SaveData(string NameValue, int TC_LeadTypeId)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            TC_InquiryType obj = new TC_InquiryType();
            string msg = "";

            if (db.TC_InquiryType.Any(o => o.InquiryType == NameValue && o.TC_LeadTypeId == TC_LeadTypeId && o.OrgInfoId == orgId))
            {
                return msg = "Same Name Alerdy Exist";
            }
            else
            {

                obj.OrgInfoId = orgId;
                obj.InquiryType = NameValue;
                obj.TC_LeadTypeId = TC_LeadTypeId;
                obj.CreatedDate = DateTime.Now; obj.CreatedBy = UserName.ToString(); obj.IsActive = true;
                db.TC_InquiryType.Add(obj);
                db.SaveChanges();
                msg = "Data Saved";
            }
            return msg;
        }
        //Get Dept By Id
        [HttpGet]
        public JsonResult getDataWithId(int ID)
        {
            var datas = db.TC_InquiryType.Where(o => o.TC_InquiryTypeId == ID).ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateData(string DataValue, int IIds, int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.TC_InquiryType.FirstOrDefault(b => b.TC_InquiryTypeId == ID);
            if (original != null)
            {
                original.InquiryType = DataValue; original.TC_LeadTypeId = IIds; original.ModifiedBy = UserName.ToString(); original.ModifiedDate = DateTime.Now;
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
            TC_InquiryType removeData = db.TC_InquiryType.Find(ID);
            if (removeData != null)
            {
                db.TC_InquiryType.Remove(removeData);
                db.SaveChanges();
                msg = "Deleted";
            }
            else
            {
                msg = "Unable to Deleted";
            }
            return msg;
        }
    }
}
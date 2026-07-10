using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class LeadDispositionMasterController : Controller
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
        public JsonResult getLeadType()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.TC_LeadType
                           where (Dis.OrgInfoId == orgId && Dis.IsActive == true)
                           select new { Dis.TC_LeadTypeId, Dis.LeadType }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getInquiryType(int IDs)
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var InquiryType = db.TC_InquiryType.Where(o => o.TC_LeadTypeId == IDs && o.OrgInfoId == orgId);
            return Json(InquiryType, JsonRequestBehavior.AllowGet);
        }
        //Get All Department
        [HttpGet]
        public JsonResult getDisposition()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = (from Dis in db.TC_LeadDisposition
                           join Inq in db.TC_InquiryType on Dis.TC_LeadInquiryTypeId equals Inq.TC_InquiryTypeId
                           where (Dis.OrgInfoId == orgId && Dis.IsActive == true)
                           select new { Dis.TC_LeadDispositionId, Dis.Name, Inq.InquiryType }).ToList();
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Save New Departmnt
        [HttpGet]
        public string SaveDisposition(string DispositionName, int TC_InquiryTypeId)
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            TC_LeadDisposition obj = new TC_LeadDisposition();
            string msg = "";

            if (db.TC_LeadDisposition.Any(o => o.Name == DispositionName && o.OrgInfoId == orgId))
            {
                return msg = "Same Name Alerdy Exist";
            }
            else
            {

                obj.OrgInfoId = orgId;
                obj.Name = DispositionName;
                obj.TC_LeadInquiryTypeId = TC_InquiryTypeId;
                obj.CreatedDate = DateTime.Now; obj.CreatedBy = UserName.ToString(); obj.IsActive = true;
                db.TC_LeadDisposition.Add(obj);
                db.SaveChanges();
                msg = "Data Saved";
            }
            return msg;
        }
        //Get Dept By Id
        [HttpGet]
        public JsonResult getDispositionWithId(int ID)
        {
            var listofdata = db.TC_LeadDisposition.Where(o => o.TC_LeadDispositionId == ID).ToList();
            return Json(listofdata, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateDisposition(string DispositionName, int InqID, int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.TC_LeadDisposition.FirstOrDefault(b => b.TC_LeadDispositionId == ID);
            if (original != null)
            {
                original.Name = DispositionName; original.TC_LeadInquiryTypeId = InqID; original.ModifiedBy = UserName.ToString(); original.ModifiedDate = DateTime.Now;
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
        public string DeleteDisposition(int ID)
        {
            string msg = "";
            TC_LeadDisposition removeData = db.TC_LeadDisposition.Find(ID);
            if (removeData != null)
            {
                db.TC_LeadDisposition.Remove(removeData);
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
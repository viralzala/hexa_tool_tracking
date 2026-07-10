using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class LeadTypeMasterController : Controller
    {  //******************
        //Author: Mudassar I
        //Date: 03/02/2017
        //Department Master
        //******************

        private ERPdbEntities db = new ERPdbEntities();
        // GET: DepartmentMaster
        public ActionResult Index()
        {
            return View();
        }
        //Get All Department
        [HttpGet]
        public JsonResult getLeadType()
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var Department = db.TC_LeadType.ToList().Where(o => o.OrgInfoId == orgId);
            return Json(Department, JsonRequestBehavior.AllowGet);
        }
        //Save New Departmnt
        [HttpGet]
        public string SaveLeadType(string LeadTypeName)
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            TC_LeadType obj = new TC_LeadType();
            string msg = "";

            if (db.TC_LeadType.Any(o => o.LeadType == LeadTypeName && o.OrgInfoId == orgId))
            {
                return msg = "Same Name Alerdy Exist";
            }
            else
            {
                obj.OrgInfoId = orgId;
                obj.LeadType = LeadTypeName; obj.IsActive = true;
                obj.CreatedDate = DateTime.Now; obj.ModifiedDate = DateTime.Now;
                obj.CreatedBy = Session["AppUserName"].ToString();
                db.TC_LeadType.Add(obj);
                db.SaveChanges();

                msg = "Data Saved";
            }
            return msg;
        }
        //Get Dept By Id
        [HttpGet]
        public JsonResult getLeadTypeWithId(int LeadId)
        {
            var listofdata = db.TC_LeadType.Where(o => o.TC_LeadTypeId == LeadId).ToList();
            return Json(listofdata, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateLeadType(string LeadTypeName, int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var original = db.TC_LeadType.First(b => b.TC_LeadTypeId == ID);
            if (original != null)
            {
                original.LeadType = LeadTypeName;
                original.ModifiedBy = Session["AppUserName"].ToString();
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
        public string DeleteLeadType(int ID)
        {
            string msg = "";
            TC_LeadType Fdata = db.TC_LeadType.Find(ID);
            if (Fdata != null)
            {
                db.TC_LeadType.Remove(Fdata);
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
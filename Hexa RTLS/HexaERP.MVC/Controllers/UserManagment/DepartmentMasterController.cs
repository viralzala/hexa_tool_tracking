using HexaERP.MVC.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.UserManagment
{
    public class DepartmentMasterController : Controller
    {
        //******************
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
        public JsonResult getDepartment()
        {
            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var Department = db.DepartMents.ToList().Where(o => o.OrgInfoId == orgId);
            return Json(Department, JsonRequestBehavior.AllowGet);
        }
        //Save New Departmnt
        [HttpGet]
        public string SaveDepartment(string DepartMentName)
        {
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            // var obj = JsonConvert.DeserializeObject<AppMenu>(JsonData);
            DepartMent obj = new DepartMent();
            string msg = "";

            if (db.DepartMents.Any(o => o.DepartMentName == DepartMentName && o.OrgInfoId == orgId))
            {
                return msg = "Same Name Alerdy Exist";
            }
            else
            {

                obj.OrgInfoId = orgId;
                obj.DepartMentName = DepartMentName;
                db.DepartMents.Add(obj);
                db.SaveChanges();
                msg = "Data Saved";
            }
            return msg;
        }
        //Get Dept By Id
        [HttpGet]
        public JsonResult getDepartmentWithId(int DeptId)
        {
            var listofdata = db.DepartMents.Where(o => o.DepartMentID == DeptId).ToList();
            return Json(listofdata, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateDepartment(string DepartMentName, int ID)
        {
            string msg = "";

            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            var original = db.DepartMents.FirstOrDefault(b => b.DepartMentID == ID);
            if (original != null)
            {
                original.DepartMentName = DepartMentName;
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
        public string DeleteDepartment(int ID)
        {
            string msg = "";
            DepartMent departMent = db.DepartMents.Find(ID);
            if (departMent != null)
            {
                db.DepartMents.Remove(departMent);
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
using HexaERP.MVC.Models;
using HigLabo.Mime;
using HigLabo.Net.Imap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace HexaERP.MVC.Controllers.CRM
{

    public class CRMController : Controller
    {
        //Author: Mudassar I
        private ERPdbEntities db = new ERPdbEntities();

        //
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        //[ValidateAntiForgeryToken]
        public string LeadData(string Json)
        {
            if (ModelState.IsValid)
            {
                var jData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TC_Customer>>(Json);
                //db.TC_Customer.Add(jData);
                //db.SaveChanges();
            }
            return Json;
        }

        //Lead Type
        [HttpGet]
        public JsonResult LeadType()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_LeadType
                                  .Where(b => b.OrgInfoId == orgId && b.IsActive == true)
                                   .Select(p => new { p.TC_LeadTypeId, p.LeadType }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //Inquiry Type
        [HttpGet]
        public JsonResult InquiryType(int LeadTypeId)
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_InquiryType
                                  .Where(b => b.OrgInfoId == orgId && b.TC_LeadTypeId == LeadTypeId && b.IsActive == true)
                                   .Select(p => new { p.TC_InquiryTypeId, p.InquiryType }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //Lead Disposition
        [HttpGet]
        public JsonResult LeadDisposition(int InquiryTypeId)
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_LeadDisposition
                                  .Where(b => b.OrgInfoId == orgId && b.TC_LeadInquiryTypeId == InquiryTypeId && b.IsActive == true)
                                   .Select(p => new { p.TC_LeadDispositionId, p.Name }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //Inquiry Source
        [HttpGet]
        public JsonResult InquirySource()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_InquirySource
                                  .Where(b => b.IsActive == true)
                                   .Select(p => new { p.TC_InquirySourceId, p.Source }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //ActionName
        [HttpGet]
        public JsonResult FollowUpActions()
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.TC_Action
                                  .Where(b => b.IsAction == true)
                                   .Select(p => new { p.TC_ActionId, p.ActionName }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }
        //Get Leads Data
        [HttpGet]
        public JsonResult LeadDatas()
        {

            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"].ToString();
            //Get Selected Data Accourding to Org Id
            var ObjData = (from LED in db.TC_Lead
                           join CUT in db.TC_Customer on LED.CustomerId equals CUT.TC_CustomerId into CUT_LED
                           join LTY in db.TC_LeadType on LED.LeadType equals LTY.TC_LeadTypeId into CUT_LTY
                           join LA in db.TC_Action on LED.TC_ActionId equals LA.TC_ActionId into CUT_LA
                           join ITY in db.TC_InquiryType on LED.TC_InquiryTypeId equals ITY.TC_InquiryTypeId into CUT_ITY
                           join LDP in db.TC_LeadDisposition on LED.TC_LeadDispositionId equals LDP.TC_LeadDispositionId into CUT_LDP
                           where (LED.OrgInfoID == orgId && LED.IsAction == true && LED.CreatedBy == UserName)

                           from CUT in CUT_LED.DefaultIfEmpty()
                           from LTY in CUT_LTY.DefaultIfEmpty()
                           from LA in CUT_LA.DefaultIfEmpty()
                           from ITY in CUT_ITY.DefaultIfEmpty()
                           from LDP in CUT_LDP.DefaultIfEmpty()
                           select new
                           {
                               TC_CustomerId = CUT.TC_CustomerId,
                               Name = CUT.Name,
                               LastName = CUT.LastName,
                               Gender = CUT.Gender,
                               EmailId = CUT.EmailId,
                               Contact = CUT.Contact,
                               CompanyName = CUT.CompanyName,
                               Designation = CUT.Designation,
                               Address = CUT.Address,
                               City = CUT.City,
                               State = CUT.State,
                               PinCode = CUT.PinCode,

                               LeadCreationDate = LED.LeadCreationDate,
                               CreatedBy = LED.CreatedBy,
                               TC_LeadId = LED.TC_LeadId,
                               NextFollowUpDate = LED.NextFollowUpDate,
                               NextFollowUpAssinged = LED.NextFollowUpAssinged,
                               AppointmentDate = LED.AppointmentDate,
                               LeadVerificationDate = LED.LeadVerificationDate,
                               LeadVerifiedBy = LED.LeadVerifiedBy,

                               ActionName = LA.ActionName,
                               PreActionCall = LA.PreActionCall,

                               LeadType = LTY.LeadType,

                               InquiryType = ITY.InquiryType,
                               abbreviation = ITY.abbreviation,

                               DisName = LDP.Name
                           }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //Get Leads Data
        [HttpGet]
        public JsonResult GetCustDetails(long CustIds)
        {

            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"].ToString();
            //Get Selected Data Accourding to Org Id
            var ObjData = (from LED in db.TC_Lead
                           join CUT in db.TC_Customer on LED.CustomerId equals CUT.TC_CustomerId into CUT_LED
                           join LTY in db.TC_LeadType on LED.LeadType equals LTY.TC_LeadTypeId into CUT_LTY
                           join LA in db.TC_Action on LED.TC_ActionId equals LA.TC_ActionId into CUT_LA
                           join ITY in db.TC_InquiryType on LED.TC_InquiryTypeId equals ITY.TC_InquiryTypeId into CUT_ITY
                           join LDP in db.TC_LeadDisposition on LED.TC_LeadDispositionId equals LDP.TC_LeadDispositionId into CUT_LDP
                           join ISR in db.TC_InquirySource on LED.TC_InquirySourceId equals ISR.TC_InquirySourceId into CUT_IS
                           where (LED.OrgInfoID == orgId && LED.IsAction == true && LED.CustomerId == CustIds)

                           from CUT in CUT_LED.DefaultIfEmpty()
                           from LTY in CUT_LTY.DefaultIfEmpty()
                           from LA in CUT_LA.DefaultIfEmpty()
                           from ITY in CUT_ITY.DefaultIfEmpty()
                           from LDP in CUT_LDP.DefaultIfEmpty()
                           from ISR in CUT_IS.DefaultIfEmpty()
                           select new
                           {
                               CUT.TC_CustomerId,
                               CUT.Name,
                               CUT.LastName,
                               CUT.Gender,
                               CUT.EmailId,
                               CUT.Contact,
                               CUT.CompanyName,
                               CUT.Designation,
                               CUT.Address,
                               CUT.City,
                               CUT.State,
                               CUT.PinCode,
                               LED.LeadType,
                               LED.TC_InquiryTypeId,
                               LED.TC_LeadDispositionId,
                               LED.TC_InquirySourceId

                           }).ToList();
            //Convert List Data to The Json Array
            return Json(ObjData.OrderByDescending(o => o.TC_CustomerId), JsonRequestBehavior.AllowGet);
        }

        //Get FollowUpData
        [HttpGet]
        public JsonResult FollowUpData(int LeadId)
        {
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"].ToString();
            //Get Selected Data Accourding to Org Id
            var ObjData = (from FL in db.TC_LeadFollowUp
                           join AC in db.TC_Action on FL.TC_ActionId equals AC.TC_ActionId
                           where (FL.OrgInfoID == orgId && FL.IsAction == true && FL.FollowUpAssinged == UserName && FL.TC_LeadId == LeadId)
                           select new
                           {
                               ActionName = AC.ActionName,
                               PostActionCall = AC.PostActionCall,
                               FollowUpDate = FL.FollowUpDate,
                               Title = FL.Title,
                               Comments = FL.Comments,
                               FollowUpAssinged = FL.FollowUpAssinged
                           });
            //Convert List Data to The Json Array
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //Craete Lead Contact
        [HttpGet]
        public string Save(string Json)
        {
            string Msg = "";
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("x");

                //DeSerialize
                var obj = JsonConvert.DeserializeObject<LeadEntity>(Json);

                if (obj.Contact == "")
                {
                    Msg = "Contact Number Is Require";
                    return Msg;
                }

                using (var context = new ERPdbEntities())
                {
                    var ObData = context.TC_Customer
                                    .Where(b => b.Contact == obj.Contact)
                                     .Select(p => new { p.TC_CustomerId, p.Uid }).ToList();
                    //var FoundId = ObData.TC_CustomerId;                    
                    if (ObData.Any())
                    {
                        var FoundId = ObData.Single();
                        Msg = "Contact Number Alredy Exist";
                    }
                    else
                    {
                        var cFlag = context.Database.ExecuteSqlCommand(
                              "INSERT INTO TC_Customer(Uid,Name,LastName,Gender,EmailId,Contact,CompanyName,Designation,Address,City,State,PinCode)" +
                              "VALUES('" + Uid + "','" + obj.Name + "','" + obj.LastName + "','" + obj.Gender.Trim() + "','" + obj.EmailId + "','" + obj.Contact + "','" + obj.CompanyName + "','" + obj.Designation + "','" + obj.Address + "','" + obj.City + "','" + obj.State + "','" + obj.PinCode + "')");
                        if (cFlag == 1)
                        {
                            //Get
                            var cData = context.TC_Customer
                                            .Where(b => b.Uid == Uid)
                                             .Select(p => new { p.TC_CustomerId, p.Uid }).ToList();
                            var cFoundId = cData.Single();

                            var lFlag = context.Database.ExecuteSqlCommand(
                              "INSERT INTO TC_Lead(CustomerId,TC_InquirySourceId,TC_InquiryTypeId,LeadCreationDate,LeadType,TC_LeadDispositionId,CreatedDate,CreatedBy,OrgInfoID) " +
                              "VALUES('" + cFoundId.TC_CustomerId + "','" + obj.TC_InquirySourceId + "','" + obj.TC_InquiryTypeId + "',GETDATE(),'" + obj.TC_LeadTypeId + "','" + obj.TC_LeadDispositionId + "',GETDATE(),'" + UserName + "','" + orgId + "')");
                            db.SaveChanges();
                            Msg = "Record Saved";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Msg = ex.Message.ToString();
            }
            return Msg;
        }
        //Craete Lead Contact
        [HttpGet]
        public string LeadFollowup(string JsonData)
        {
            string Msg = "";
            try
            {
                //Users Info
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                //DeSerialize
                var obj = JsonConvert.DeserializeObject<LeadFollowUpEntity>(JsonData);

                var cFlag = db.Database.ExecuteSqlCommand(
                             "INSERT INTO TC_LeadFollowUp(TC_LeadId,TC_ActionId,FollowUpDate,Title,Comments,FollowUpAssinged,OrgInfoID,CreatedDate,CreatedBy)" +
                             "VALUES('" + obj.TC_LeadId + "','" + obj.TC_ActionId + "','" + Convert.ToDateTime(obj.FollowUpDate) + "','" + obj.Title + "','" + obj.Comments + "','" + UserName + "','" + orgId + "',getdate(),'" + UserName + "')");
                if (cFlag == 1)
                {
                    Msg = "Follow Up Data Saved Sucessfully ";
                }
                else
                {
                    Msg = "Something Went Wrongs ";
                }
            }
            catch (Exception ex)
            {
                Msg = ex.Message.ToString();
            }

            return Msg;
        }

        //Next Foloow Up
        [HttpGet]
        public string NextFollowup(string JsonData)
        {
            string Msg = "";
            try
            {
                //Users Info
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                //DeSerialize
                var obj = JsonConvert.DeserializeObject<NextFollowUpEntity>(JsonData);

                var cFlag = db.Database.ExecuteSqlCommand("UPDATE TC_Lead SET TC_ActionId='" + obj.TC_ActionId + "',NextFollowUpDate='" + Convert.ToDateTime(obj.NextFollowUpDate) + "',NextFollowUpAssinged='" + UserName + "' WHERE TC_LeadId='" + obj.TC_LeadId + "' AND OrgInfoID='" + orgId + "'");
                if (cFlag == 1)
                {
                    //var FLFlag = db.Database.ExecuteSqlCommand(
                    //       "INSERT INTO TC_LeadFollowUp(TC_LeadId,TC_ActionId,FollowUpDate,FollowUpAssinged,OrgInfoID,CreatedDate,CreatedBy)" +
                    //       "VALUES('" + obj.TC_LeadId + "','" + obj.TC_ActionId + "','" + Convert.ToDateTime(obj.NextFollowUpDate) + "','" + UserName + "','" + orgId + "',getdate(),'" + UserName + "')");
                    Msg = "Next Follow Up Date Assined Successfully";
                }
                else
                {
                    Msg = "Something Went Wrongs ";
                }
            }
            catch (Exception ex)
            {
                Msg = ex.Message.ToString();
            }

            return Msg;
        }

        //Appointment
        [HttpGet]
        public string Appointment(string JsonData)
        {
            string Msg = "";
            try
            {
                //Users Info
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                //DeSerialize
                var obj = JsonConvert.DeserializeObject<AppointmentEntity>(JsonData);

                var cFlag = db.Database.ExecuteSqlCommand("UPDATE TC_Lead SET AppointmentDate='" + Convert.ToDateTime(obj.AppointmentDate) + "' WHERE TC_LeadId='" + obj.TC_LeadId + "' AND OrgInfoID='" + orgId + "'");
                if (cFlag == 1)
                {
                    Msg = "Appointment Scheduled On :" + Convert.ToDateTime(obj.AppointmentDate).ToString();
                }
                else
                {
                    Msg = "Something Went Wrongs ";
                }
            }
            catch (Exception ex)
            {
                Msg = ex.Message.ToString();
            }

            return Msg;
        }

        //Craete Lead Contact
        [HttpGet]
        public string EditData(string JsonData, int CustIds)
        {
            string Msg = "";
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("x");

                //DeSerialize
                var obj = JsonConvert.DeserializeObject<LeadEntity>(JsonData);
                var cFlag = db.Database.ExecuteSqlCommand(
                             "UPDATE TC_Customer SET Name='" + obj.Name + "',LastName='" + obj.LastName + "',Gender='" + obj.Gender + "',EmailId='" + obj.EmailId + "',Contact='" + obj.Contact + "',CompanyName='" + obj.CompanyName + "',Designation='" + obj.Designation + "',Address='" + obj.Address + "',City='" + obj.City + "',State='" + obj.State + "',PinCode='" + obj.PinCode + "' " +
                             "WHERE TC_CustomerId='" + CustIds + "'");
                if (cFlag == 1)
                {
                    var lFlag = db.Database.ExecuteSqlCommand(
                              "UPDATE TC_Lead SET TC_InquirySourceId='" + obj.TC_InquirySourceId + "',TC_InquiryTypeId='" + obj.TC_InquiryTypeId + "',LeadType='" + obj.TC_LeadTypeId + "',TC_LeadDispositionId='" + obj.TC_LeadDispositionId + "',ModifiedDate=GETDATE(),ModifiedBy='" + UserName + "',OrgInfoID='" + orgId + "' " +
                              "WHERE CustomerId='" + CustIds + "'");
                    Msg = lFlag.ToString();
                }

            }
            catch (Exception ex)
            {
                Msg = ex.Message.ToString();
            }
            return Msg;
        }

        //Get Leads Data
        [HttpGet]
        public JsonResult Getmails()
        {
            var model = new List<EmailEntity>();
            List<EmailEntity> newMessages = new List<EmailEntity>();
            try
            {
                //Get Organization Id From Session Variable
                int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"].ToString();
                //Get Selected Data Accourding to Org Id
                var ObjData = db.TC_EmailConfiguration
                                      .Where(b => b.OrgInfoId == orgId && b.CreatedBy == UserName && b.IsAction == true)
                                       .Select(p => new { p.FromEmailId, p.EmailServerName, p.FromName, p.LoginPassword, p.Port, p.IsSSL }).ToList();
                if (ObjData.Any())
                {
                    var FoundId = ObjData.Single();

                    MailMessage mg = null;
                    using (ImapClient cl = new ImapClient(FoundId.EmailServerName, FoundId.Port, FoundId.FromEmailId, FoundId.LoginPassword))
                    {
                        cl.Ssl = true;
                        var bl = cl.TryAuthenticate();
                        if (bl == true)
                        {
                            //Select folder
                            var folder = cl.SelectFolder("INBOX");

                            //Get all mail from folder
                            for (int i = 0; i < folder.MailCount; i++)
                            {
                                mg = cl.GetMessage(i + 1);

                                //obj.Date = Convert.ToDateTime(mg.Date.DateTime);
                                //obj.SenderAddress = mg.Headers.From.Value;
                                //obj.Subject = mg.Subject;
                                //obj.HtmlDataText = mg.BodyHtml;
                                //newMessages.Add(obj);

                                model.Add(new EmailEntity
                                {
                                    Date = Convert.ToDateTime(mg.Date.DateTime),
                                    SenderAddress = mg.Headers.From.Value,
                                    Subject = mg.Subject,
                                    HtmlDataText = mg.BodyHtml,
                                    SenderName = mg.Headers.From.DisplayName,
                                    MessageID = mg.MessageID
                                });
                            }
                            //var s = Json(rivers, JsonRequestBehavior.AllowGet);

                        }

                    }
                }
                else
                {

                }
                //EmailEntity obj = new EmailEntity();

            }
            catch (Exception)
            {
                //Response.Write("<scriprt>alert('" + ex.Message + "')</scriprt>");
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //Craete Lead Contact
        [HttpGet]
        public string QuickLeadSave(string JsonData)
        {
            string Msg = "";
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("x");

                //DeSerialize
                var obj = JsonConvert.DeserializeObject<LeadEntity>(JsonData);

                if (obj.Contact == "")
                {
                    Msg = "Contact Number Is Require";
                    return Msg;
                }
                using (var context = new ERPdbEntities())
                {
                    var ObData = context.TC_Customer
                                    .Where(b => b.Contact == obj.Contact)
                                     .Select(p => new { p.TC_CustomerId, p.Uid }).ToList();
                    //var FoundId = ObData.TC_CustomerId;                    
                    if (ObData.Any())
                    {
                        var FoundId = ObData.Single();
                        Msg = "Contact Number Alredy Exist";
                    }
                    else
                    {
                        var cFlag = context.Database.ExecuteSqlCommand(
                              "INSERT INTO TC_Customer(Uid,Name,LastName,EmailId,Contact)" +
                              "VALUES('" + Uid + "','" + obj.Name.Trim() + "','" + obj.LastName.Trim() + "','" + obj.EmailId + "','" + obj.Contact + "')");
                        if (cFlag == 1)
                        {
                            //Get
                            var cData = context.TC_Customer
                                            .Where(b => b.Uid == Uid)
                                             .Select(p => new { p.TC_CustomerId, p.Uid }).ToList();
                            var cFoundId = cData.Single();

                            var lFlag = context.Database.ExecuteSqlCommand(
                              "INSERT INTO TC_Lead(CustomerId,TC_InquirySourceId,TC_InquiryTypeId,LeadCreationDate,LeadType,TC_LeadDispositionId,CreatedDate,CreatedBy,OrgInfoID) " +
                              "VALUES('" + cFoundId.TC_CustomerId + "','" + obj.TC_InquirySourceId + "','" + obj.TC_InquiryTypeId + "',GETDATE(),'" + obj.TC_LeadTypeId + "','" + obj.TC_LeadDispositionId + "',GETDATE(),'" + UserName + "','" + orgId + "')");
                            db.SaveChanges();
                            Msg = "Record Saved";
                        }
                    }
                }//end 
            }
            catch (Exception ex)
            {
                Msg = ex.Message.ToString();
            }
            return Msg;
        }
    }
}

using HexaERP.MVC.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.CRM
{
    public class EmailConfigurationController : Controller
    {
        //Author: Mudassar I
        private ERPdbEntities db = new ERPdbEntities();
        // GET: EmailConfiguration
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


        /// <summary>
        /// Adding New mail Configuration
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Config(mMailConfig _obj)
        {
            JsonResult json = new JsonResult();
            try
            {
                if (string.IsNullOrEmpty(_obj.EmailId))
                {
                    return json = this.Json(new { Flag = false, Message = "Enter Email" }, JsonRequestBehavior.AllowGet);
                }
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                if (_obj.IsOut == true)
                {
                    if (!string.IsNullOrEmpty(_obj.EmailId) && !string.IsNullOrEmpty(_obj.EmailServer) && !string.IsNullOrEmpty(_obj.Password))
                    {
                        var _chkDub = db.mMailConfigs.Where(x => x.IsOut == true && x.IsAction == true).ToList();
                        if (_chkDub.Count > 0)
                        {
                            json = this.Json(new { Flag = false, Message = "outgoing mail alredy exist/ disbale existing mail to add new outgoing mail" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            _obj.CreatedBy = UserName.ToString();
                            _obj.OrgInfoId = orgId;
                            _obj.CreatedDate = DateTime.Now;
                            db.mMailConfigs.Add(_obj);
                            db.SaveChanges();
                            NotifyMailer(_obj.EmailId, _obj.Password, _obj.EmailServer, _obj.Port, _obj.IsSSL);
                            json = this.Json(new { Flag = true, Message = "Successfully Added!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else { json = this.Json(new { Flag = false, Message = "You Missing Required Data" }, JsonRequestBehavior.AllowGet); }
                }
                else if (_obj.IsOut == false)
                {
                    if (!string.IsNullOrEmpty(_obj.EmailId))
                    {
                        var _chkDub = db.mMailConfigs.Where(x => x.EmailId == _obj.EmailId).ToList();
                        if (_chkDub.Count > 0)
                        {
                            json = this.Json(new { Flag = false, Message = "Already Same Email Exist" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            _obj.CreatedBy = UserName.ToString();
                            _obj.OrgInfoId = orgId;
                            _obj.CreatedDate = DateTime.Now;
                            db.mMailConfigs.Add(_obj);
                            db.SaveChanges();
                            NotifyMailer(_obj.EmailId, _obj.Password, _obj.EmailServer, _obj.Port, _obj.IsSSL);
                            json = this.Json(new { Flag = true, Message = "Successfully Added!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                json = this.Json(new { Flag = false, ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return json;
        }

        /// <summary>
        /// Updateing Mail Record
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateConfig(mMailConfig _obj)
        {
            JsonResult json = new JsonResult();

            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);

                if (string.IsNullOrEmpty(_obj.mMailConfigId.ToString()))
                {
                    return json = this.Json(new { Flag = false, Message = "Somethimg Went Wrong Please Try Again." }, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(_obj.EmailId))
                {
                    return json = this.Json(new { Flag = false, Message = "Enter Email" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var _getInfo = db.mMailConfigs.Where(x => x.mMailConfigId == _obj.mMailConfigId).FirstOrDefault();

                    if (_getInfo != null)
                    {
                        if (_getInfo.IsOut.Equals(true))
                        {

                            _getInfo.EmailId = _obj.EmailId;
                            _getInfo.DisplayName = _obj.DisplayName;
                            _getInfo.IsAction = _obj.IsAction;
                            _getInfo.EmailServer = _obj.EmailServer;
                            _getInfo.Password = _obj.Password;
                            _getInfo.Port = _obj.Port;
                            _getInfo.IsSSL = _obj.IsSSL;
                            _getInfo.ModifiedBy = UserName.ToString(); _getInfo.ModifiedDate = DateTime.Now;
                            db.Entry(_getInfo).State = EntityState.Modified;
                            db.SaveChanges();
                            json = this.Json(new { Flag = true, Message = "Outgoing mail updated!" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (_getInfo.IsOut.Equals(false))
                        {
                            _getInfo.EmailId = _obj.EmailId;
                            _getInfo.DisplayName = _obj.DisplayName;
                            _getInfo.IsAction = _obj.IsAction;

                            _getInfo.IsAdd = _obj.IsAdd;
                            _getInfo.IsModify = _obj.IsModify;
                            _getInfo.IsDelete = _obj.IsDelete;
                            _getInfo.IsNotify = _obj.IsNotify;

                            _getInfo.ModifiedBy = UserName.ToString(); _getInfo.ModifiedDate = DateTime.Now;
                            db.Entry(_getInfo).State = EntityState.Modified;
                            db.SaveChanges();
                            json = this.Json(new { Flag = true, Message = "Incomming mail updated!" }, JsonRequestBehavior.AllowGet);
                        }
                        else { json = this.Json(new { Flag = false, Message = "Some think went wrong. re login to application" }, JsonRequestBehavior.AllowGet); }
                    }
                    else { return json = this.Json(new { Flag = false, Message = "Record Not Found" }, JsonRequestBehavior.AllowGet); }
                }
            }
            catch (Exception ex)
            {
                json = this.Json(new { Flag = false, ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return json;
        }


        private void NotifyMailer(string _email, string _password, string _proto, int _port, bool? _ssl)
        {
            //string body;
            String text;

            try
            {
                //string text;
                var fileStream = new FileStream(Server.MapPath("\\App_Data\\Template\\EmailTemplete.txt"), FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    text = streamReader.ReadToEnd();
                }

                string messageBody = string.Format(text, "Mudasasr I", DateTime.Now.ToString());
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(_proto);
                mail.From = new MailAddress(_email);
                mail.To.Add(_email);
                mail.Subject = "Your Mail Configured Successfully";
                mail.IsBodyHtml = true;
                string htmlBody;
                htmlBody = messageBody;
                string returnUrl = Server.UrlDecode(htmlBody);
                mail.Body = returnUrl;
                SmtpServer.Port = _port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(_email, _password);
                SmtpServer.EnableSsl = Convert.ToBoolean(_ssl);
                SmtpServer.Send(mail);

            }
            catch (Exception)
            {
                // Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }
        }


        //Craete Not USing
        [HttpGet]
        public string Save(string Json)
        {
            string msg = "";
            try
            {
                String text;
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                //DeSerialize
                var obj = JsonConvert.DeserializeObject<TC_EmailConfiguration>(Json);
                if (db.TC_EmailConfiguration.Any(o => o.OrgInfoId == orgId && o.CreatedBy == UserName.ToString() && obj.IsAction == true))
                {
                    return msg = "Same Name Alerdy Exist";
                }
                else
                {
                    obj.CreatedBy = UserName.ToString(); obj.OrgInfoId = orgId;
                    obj.CreatedDate = DateTime.Now;
                    db.TC_EmailConfiguration.Add(obj);
                    db.SaveChanges();
                    //MailController on = new MailController();
                    //var objE = new FmailClass
                    //{
                    //    FromEmailId = obj.FromEmailId                    

                    //};
                    //var json = new JavaScriptSerializer().Serialize(objE);

                    //on.demo(json, "Your Mail Configured","Mail Setup In Hexa ERP");
                    // string body;
                    //Read template file from the App_Data folder
                    //using (StreamReader reader = File.OpenText("D:\\report.html"))
                    //{
                    //    mail.Body = reader.ReadToEnd();
                    //}
                    //StreamReader _sr;
                    //using (_sr = new StreamReader(Server.MapPath("\\App_Data\\Templates\")" + "EmailTemplete.txt"))
                    //{
                    //    body = _sr.ReadToEnd();
                    //}

                    try
                    {
                        //string text;
                        var fileStream = new FileStream(Server.MapPath("\\App_Data\\Template\\EmailTemplete.txt"), FileMode.Open, FileAccess.Read);
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            text = streamReader.ReadToEnd();
                        }

                        string messageBody = string.Format(text, "Mudasasr I", DateTime.Now.ToString());

                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                        mail.From = new MailAddress(obj.FromEmailId);
                        mail.To.Add(obj.FromEmailId);
                        mail.Subject = "Your Mail Configured Successfully";
                        mail.IsBodyHtml = true;
                        string htmlBody;
                        htmlBody = messageBody;
                        string returnUrl = Server.UrlDecode(htmlBody);
                        mail.Body = returnUrl;
                        SmtpServer.Port = 587;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(obj.FromEmailId, obj.LoginPassword);
                        SmtpServer.EnableSsl = true;
                        SmtpServer.Send(mail);
                        msg = "Same Sucessfull";
                        // string _uu = text;
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        //Console.WriteLine("Executing finally block.");
                    }


                }
            }
            catch (DbEntityValidationException ex)
            {
                msg = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(msg);
            }
            return msg;
        }

        public class FmailClass
        {
            public string FromEmailId;
        }

        //Get All Department
        [HttpGet]
        public JsonResult getData()
        {
            var UserName = Session["AppUserName"];
            //Get Organization Id From Session Variable
            int orgId = Convert.ToInt32(Session["OrgInfoId"]);
            //Get Selected Data Accourding to Org Id
            var ObjData = db.mMailConfigs.Where(o => o.OrgInfoId == orgId && o.CreatedBy == UserName.ToString());
            //Convert List Data to The Json Array          
            return Json(ObjData, JsonRequestBehavior.AllowGet);
        }

        //Get Dept By Id
        [HttpGet]
        public JsonResult getDataWithId(int ID)
        {
            var datas = db.mMailConfigs.Where(o => o.mMailConfigId == ID).ToList();
            return Json(datas, JsonRequestBehavior.AllowGet);
        }
        //Update Department
        [HttpGet]
        public string UpdateData(string formData, int ID)
        {
            string msg = "";
            var obj = JsonConvert.DeserializeObject<TC_EmailConfiguration>(formData);
            int orgId = Convert.ToInt32(Session["OrgInfoId"]); var UserName = Session["AppUserName"];
            var original = db.TC_EmailConfiguration.FirstOrDefault(b => b.EmailConfigurationId == ID);
            if (original != null)
            {
                original.FromEmailId = obj.FromEmailId;
                original.EmailServerName = obj.EmailServerName;
                original.FromName = obj.FromName;
                original.LoginPassword = obj.LoginPassword;
                original.Port = obj.Port;
                original.IsSSL = obj.IsSSL;
                original.IsAction = obj.IsAction;
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
            mMailConfig removeData = db.mMailConfigs.Find(ID);
            if (removeData != null)
            {
                db.mMailConfigs.Remove(removeData);
                db.SaveChanges();
                msg = "Deleted";
            }
            else
            {
                msg = "Unable to Deleted";
            }
            return msg;
        }

        [HttpGet]
        public void googleEvent()
        {
            //    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //                    new ClientSecrets
            //                    {
            //                        ClientId = "CLIENT ID",
            //                        ClientSecret = "CLIENT SECRET",
            //                    },
            //                    new[] { CalendarService.Scope.Calendar },
            //                    "user",
            //                    CancellationToken.None).Result;

            //    // Create the service.
            //    var service = new CalendarService(new BaseClientService.Initializer
            //    {
            //        HttpClientInitializer = credential,
            //        ApplicationName = "Calendar API Sample",
            //    });

            //    var myEvent = new Event
            //    {
            //        Summary = "Google Calendar Api Sample Code by Mukesh Salaria",
            //        Location = "Gurdaspur, Punjab, India",
            //        Start = new EventDateTime
            //        {
            //            DateTime = new DateTime(2015, 3, 2, 6, 0, 0),
            //        },
            //        End = new EventDateTime
            //        {
            //            DateTime = new DateTime(2015, 3, 2, 7, 30, 0),
            //        },
            //        Recurrence = new String[] { "RRULE:FREQ=WEEKLY;BYDAY=MO" },

            //        Attendees = new List
            //        {
            //             new EventAttendee { Email = "programmer.mukesh01@gmail.com"}
            //        },
            //    };

            //    var recurringEvent = service.Events.Insert(myEvent, "primary");
            //    recurringEvent.SendNotifications = true;
            //    recurringEvent.Execute();
        }

        //[HttpGet]
        //public void EventCreate()
        //{
        //    // Refer to the .NET quickstart on how to setup the environment:
        //    // https://developers.google.com/google-apps/calendar/quickstart/dotnet
        //    // Change the scope to CalendarService.Scope.Calendar and delete any stored
        //    // credentials.

        //    Event newEvent = new Event()
        //    {
        //        Summary = "Google I/O 2015",
        //        Location = "800 Howard St., San Francisco, CA 94103",
        //        Description = "A chance to hear more about Google's developer products.",
        //        Start = new EventDateTime()
        //        {
        //            DateTime = DateTime.Parse("2015-05-28T09:00:00-07:00"),
        //            TimeZone = "America/Los_Angeles",
        //        },
        //        End = new EventDateTime()
        //        {
        //            DateTime = DateTime.Parse("2015-05-28T17:00:00-07:00"),
        //            TimeZone = "America/Los_Angeles",
        //        },
        //        Recurrence = new String[] { "RRULE:FREQ=DAILY;COUNT=2" },
        //        Attendees = new EventAttendee[] {
        //new EventAttendee() { Email = "lpage@example.com" },
        //new EventAttendee() { Email = "sbrin@example.com" },
        //              },
        //        Reminders = new Event.RemindersData()
        //        {
        //            UseDefault = false,
        //            Overrides = new EventReminder[] {
        //    new EventReminder() { Method = "email", Minutes = 24 * 60 },
        //    new EventReminder() { Method = "sms", Minutes = 10 },
        //}
        //        }
        //    };

        //    String calendarId = "primary";
        //    EventsResource.InsertRequest request = service.Events.Insert(newEvent, calendarId);
        //    Event createdEvent = request.Execute();
        //    Console.WriteLine("Event created: {0}", createdEvent.HtmlLink);
        //}
    }
}
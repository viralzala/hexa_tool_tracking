using HexaERP.MVC.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace HexaERP.MVC.EmailConfig
{
    //private Entities db = new Entities();

    public static class NotifyMail
    {
        static ERPdbEntities db = new ERPdbEntities();
        public static void AddNotify(string _assetName, string _serialNo, string _useradded, DateTime _dateTime, string _subject)
        {
            // string body;
            String text;

            var _pushmail = db.mMailConfigs.Where(x => x.IsOut == true && x.IsAction == true).FirstOrDefault();

            if (_pushmail != null)
            {
                var _mailList = db.mMailConfigs.Where(x => x.IsAdd == true && x.IsAction == true).ToList();

                if (_mailList.Count > 0)
                {
                    var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath("\\App_Data\\Template\\AddTemplete.txt"), FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        text = streamReader.ReadToEnd();
                    }
                    string messageBody = string.Format(text, _assetName, _serialNo, _useradded, _dateTime);
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    message.From = new MailAddress(_pushmail.EmailId.ToString());
                    foreach (var _eid in _mailList)
                    {
                        message.To.Add(new MailAddress(_eid.EmailId));
                    }
                    try
                    {
                        string htmlBody;
                        htmlBody = messageBody;
                        string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        message.Body = returnUrl;
                        message.Subject = _subject;
                        message.IsBodyHtml = true;
                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        ///smtp server and port configured at registry
                        SmtpClient smtpClient = new SmtpClient(_pushmail.EmailServer, _pushmail.Port);
                        ///enable ssl is required for secure connection.It is must be true for gmail server and false for other servers.
                        smtpClient.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        smtpClient.Send(message);


                        //// Your Exchange Server address
                        //SmtpServer oServer = new SmtpServer("exch.emailarchitect.net");

                        //// Set Exchange Web Service EWS - Exchange 2007/2010/2013/2016
                        //oServer.Protocol = ServerProtocol.ExchangeEWS;
                        //// User and password for Exchange authentication
                        //oServer.User = "test";
                        //oServer.Password = "testpassword";

                        //// By default, Exchange Web Service requires SSL connection
                        //oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                        //// Create instance of ExchangeClient class by giving credentials
                        //ExchangeClient client = new ExchangeClient("http://MachineName/exchange/username","username", "password", "domain");
                        //// Create instance of type MailMessage
                        //MailMessage msg = new MailMessage();
                        //msg.From = "sender@domain.com";
                        //msg.To = "recipient@ domain.com ";
                        //msg.Subject = "Sending message from exchange server";
                        //msg.HtmlBody = "  sending message from exchange server";
                        //// Send the message
                        //client.Send(msg);

                        //MailMessage mail = new MailMessage();
                        //SmtpClient SmtpServer = new SmtpClient(_pushmail.EmailServer);
                        //mail.From = new MailAddress(_pushmail.EmailId);
                        //mail.To.Add(_eid.EmailId);
                        //mail.Subject = _subject;
                        //mail.IsBodyHtml = true;
                        //string htmlBody;
                        //htmlBody = messageBody;
                        //string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        //mail.Body = returnUrl;
                        //SmtpServer.Port = _pushmail.Port;
                        //SmtpServer.Credentials = new System.Net.NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        //SmtpServer.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        //SmtpServer.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        string[] errorlines = { "Method:" + "Add Notify", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.Message + Environment.NewLine };
                        File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);
                    }
                    finally
                    {
                        //Console.WriteLine("Executing finally block.");
                    }
                }
            }
        }

        public static void ModifyNotify(string _assetName, string _serialNo, string _useradded, DateTime _dateTime, string _subject)
        {
            //string body;
            String text;

            var _pushmail = db.mMailConfigs.Where(x => x.IsOut == true && x.IsAction == true).FirstOrDefault();

            if (_pushmail != null)
            {
                var _mailList = db.mMailConfigs.Where(x => x.IsModify == true && x.IsAction == true).ToList();

                if (_mailList.Count > 0)
                {

                    var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath("\\App_Data\\Template\\AddTemplete.txt"), FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        text = streamReader.ReadToEnd();
                    }
                    string messageBody = string.Format(text, _assetName, _serialNo, _useradded, _dateTime);
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    message.From = new MailAddress(_pushmail.EmailId.ToString());
                    foreach (var _eid in _mailList)
                    {
                        message.To.Add(new MailAddress(_eid.EmailId));
                    }
                    try
                    {
                        string htmlBody;
                        htmlBody = messageBody;
                        string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        message.Body = returnUrl;
                        message.Subject = _subject;
                        message.IsBodyHtml = true;
                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        ///smtp server and port configured at registry
                        SmtpClient smtpClient = new SmtpClient(_pushmail.EmailServer, _pushmail.Port);
                        ///enable ssl is required for secure connection.It is must be true for gmail server and false for other servers.
                        smtpClient.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        smtpClient.Send(message);

                        //MailMessage mail = new MailMessage();
                        //SmtpClient SmtpServer = new SmtpClient(_pushmail.EmailServer);
                        //mail.From = new MailAddress(_pushmail.EmailId);
                        //mail.To.Add(_eid.EmailId);
                        //mail.Subject = _subject;
                        //mail.IsBodyHtml = true;
                        //string htmlBody;
                        //htmlBody = messageBody;
                        //string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        //mail.Body = returnUrl;
                        //SmtpServer.Port = _pushmail.Port;
                        //SmtpServer.Credentials = new System.Net.NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        //SmtpServer.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        //SmtpServer.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        string[] errorlines = { "Method:" + "Modify Notify", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.Message + Environment.NewLine };
                        File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);
                    }
                    finally
                    {
                        //Console.WriteLine("Executing finally block.");
                    }

                }

            }

        }

        public static void DeleteNotify(string _assetName, string _serialNo, string _useradded, DateTime _dateTime, string _subject)
        {
            //string body;
            String text;

            var _pushmail = db.mMailConfigs.Where(x => x.IsOut == true && x.IsAction == true).FirstOrDefault();

            if (_pushmail != null)
            {
                var _mailList = db.mMailConfigs.Where(x => x.IsDelete == true && x.IsAction == true).ToList();

                if (_mailList.Count > 0)
                {
                    var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath("\\App_Data\\Template\\AddTemplete.txt"), FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        text = streamReader.ReadToEnd();
                    }
                    string messageBody = string.Format(text, _assetName, _serialNo, _useradded, _dateTime);
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    message.From = new MailAddress(_pushmail.EmailId.ToString());
                    foreach (var _eid in _mailList)
                    {
                        message.To.Add(new MailAddress(_eid.EmailId));
                    }
                    try
                    {
                        string htmlBody;
                        htmlBody = messageBody;
                        string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        message.Body = returnUrl;
                        message.Subject = _subject;
                        message.IsBodyHtml = true;
                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        ///smtp server and port configured at registry
                        SmtpClient smtpClient = new SmtpClient(_pushmail.EmailServer, _pushmail.Port);
                        ///enable ssl is required for secure connection.It is must be true for gmail server and false for other servers.
                        smtpClient.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        smtpClient.Send(message);

                        //MailMessage mail = new MailMessage();
                        //SmtpClient SmtpServer = new SmtpClient(_pushmail.EmailServer);
                        //mail.From = new MailAddress(_pushmail.EmailId);
                        //mail.To.Add(_eid.EmailId);
                        //mail.Subject = _subject;
                        //mail.IsBodyHtml = true;
                        //string htmlBody;
                        //htmlBody = messageBody;
                        //string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        //mail.Body = returnUrl;
                        //SmtpServer.Port = _pushmail.Port;
                        //SmtpServer.Credentials = new System.Net.NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        //SmtpServer.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        //SmtpServer.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        string[] errorlines = { "Method:" + "Delete Notify", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.Message + Environment.NewLine };
                        File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);
                    }
                    finally
                    {
                        //Console.WriteLine("Executing finally block.");
                    }
                }
            }
        }


        public static void TrasactionNotify(string _assetName, string _modelNo, string _serialNo, string _employeeName, string _employeeId, string _issuedBy, DateTime? _Issuedate, DateTime? _returningdate, string _subject)
        {
            //string body;
            String text;

            var _pushmail = db.mMailConfigs.Where(x => x.IsOut == true && x.IsAction == true).FirstOrDefault();

            if (_pushmail != null)
            {
                var _mailList = db.mMailConfigs.Where(x => x.IsNotify == true && x.IsAction == true).ToList();

                if (_mailList.Count > 0)
                {
                    var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath("\\App_Data\\Template\\checkinout.txt"), FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        text = streamReader.ReadToEnd();
                    }

                    string messageBody = string.Format(text, _assetName, _modelNo, _serialNo, _employeeName, _employeeId, _issuedBy, _Issuedate.Value.Date.ToString("d"), _returningdate.Value.Date.ToString("d"));
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    message.From = new MailAddress(_pushmail.EmailId.ToString());

                    foreach (var _eid in _mailList)
                    {
                        message.To.Add(new MailAddress(_eid.EmailId));
                    }

                    try
                    {
                        string htmlBody;
                        htmlBody = messageBody;
                        string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        message.Body = returnUrl;
                        message.Subject = _subject;
                        message.IsBodyHtml = true;
                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        ///smtp server and port configured at registry
                        SmtpClient smtpClient = new SmtpClient(_pushmail.EmailServer, _pushmail.Port);
                        ///enable ssl is required for secure connection.It is must be true for gmail server and false for other servers.
                        smtpClient.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = true;
                        smtpClient.Credentials = new NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        smtpClient.Send(message);

                        //MailMessage mail = new MailMessage();
                        //SmtpClient SmtpServer = new SmtpClient(_pushmail.EmailServer);
                        //mail.From = new MailAddress(_pushmail.EmailId);
                        //mail.To.Add(_eid.EmailId);
                        //mail.Subject = _subject;
                        //mail.IsBodyHtml = true;
                        //string htmlBody;
                        //htmlBody = messageBody;
                        //string returnUrl = System.Web.HttpContext.Current.Server.UrlDecode(htmlBody);
                        //mail.Body = returnUrl;
                        //SmtpServer.Port = _pushmail.Port;
                        //SmtpServer.Credentials = new System.Net.NetworkCredential(_pushmail.EmailId, _pushmail.Password);
                        //SmtpServer.EnableSsl = Convert.ToBoolean(_pushmail.IsSSL);
                        //SmtpServer.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        string[] errorlines = { "Method:" + "Delete Notify", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.Message + Environment.NewLine };
                        File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);
                    }
                    finally
                    {
                        //Console.WriteLine("Executing finally block.");
                    }
                }
            }
        }
    }
}
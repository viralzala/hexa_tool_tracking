using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace HexaERP.MVC.Controllers.CRM
{
    public class MailController : Controller
    {
        //Author: Mudassar I
        private ERPdbEntities db = new ERPdbEntities();
        //Get Leads Data
        [HttpGet]
        public string demo(string JsonData, string Message, string Subject)
        {
            string _returnUrl = Server.UrlDecode(Message);
            string msg = "";
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
                    DataTable dt = new DataTable();
                    dt = JsonStringToDataTable(JsonData);
                    if (dt.Rows.Count > 0)
                    {

                        for (int i = 0; dt.Rows.Count > i; i++)
                        {
                            MailMessage mail = new MailMessage();
                            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                            mail.From = new MailAddress(FoundId.FromEmailId);
                            mail.To.Add(dt.Rows[i].ItemArray.GetValue(1).ToString());
                            mail.Subject = Subject;
                            mail.IsBodyHtml = true;
                            //string htmlBody;
                            //htmlBody = "<b>Write some HTML code here</b>";
                            string returnUrl = Server.UrlDecode(Message);
                            mail.Body = returnUrl;
                            SmtpServer.Port = 587;
                            SmtpServer.Credentials = new System.Net.NetworkCredential(FoundId.FromEmailId, FoundId.LoginPassword);
                            SmtpServer.EnableSsl = true;
                            SmtpServer.Send(mail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return msg = ex.Message.ToString();
            }
            return msg;
        }


        //Get Leads Data
        [HttpGet]
        public string SendMail(string JsonData, string Message, string Subject)
        {
            string msg = "";
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
                    DataTable dt = new DataTable();
                    dt = JsonStringToDataTable(JsonData);
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; dt.Rows.Count > i; i++)
                        {
                            MailAddress ma_from = new MailAddress(FoundId.FromEmailId, FoundId.FromName);
                            MailAddress ma_to = new MailAddress(dt.Rows[i].ItemArray.GetValue(0).ToString(), dt.Rows[i].ItemArray.GetValue(1).ToString());
                            string s_password = FoundId.LoginPassword;
                            string s_subject = Subject;
                            string s_body = "Dear ," + dt.Rows[i].ItemArray.GetValue(1).ToString() + " " + Message;
                            SmtpClient smtp = new SmtpClient
                            {
                                Host = "smtp.gmail.com",
                                //change the port to prt 587. This seems to be the standard for Google smtp transmissions.
                                Port = 587,
                                //enable SSL to be true, otherwise it will get kicked back by the Google server.
                                EnableSsl = true,
                                //The following properties need set as well
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(ma_from.Address, s_password)
                            };
                            using (MailMessage mail = new MailMessage(ma_from, ma_to)
                            {
                                Subject = s_subject,
                                Body = s_body
                            })
                                try
                                {
                                    smtp.Send(mail);
                                }
                                catch (Exception ex)
                                {
                                    return msg = ex.Message.ToString();
                                }
                            string email = dt.Rows[i].ItemArray.GetValue(0).ToString();
                        }
                        msg = "Mail Send Successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                return msg = ex.Message.ToString();
            }
            return msg;
        }

        //J
        public static DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");
            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString))
                        {
                            ColumnsName.Add(ColumnsNameString);
                        }
                    }
                    catch (Exception)
                    {
                        //if (log.IsErrorEnabled)
                        //{
                        //    log.Error("Page Load failed : " + ex.Message);
                        //}
                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception)
                    {
                        //if (log.IsErrorEnabled)
                        //{
                        //    log.Error("Page Load failed : " + ex.Message);
                        //}
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
    }
}
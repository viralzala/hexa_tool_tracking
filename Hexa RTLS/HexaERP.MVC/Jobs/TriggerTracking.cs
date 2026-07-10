using EntityFramework.Extensions;
using HexaERP.MVC.Models;
using HexaERP.MVC.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HexaERP.MVC.Jobs
{
    public class TriggerTracking
    {
        public void TiggerAlertJob()
        {
            try
            {
                var dateLimit = DateTime.Now;

                var _AssemblyReader = ConfigurationManager.AppSettings["Smt:AssemblyReader"].Split(',');

                var _TimerIntervalInSeconds = Convert.ToDouble(ConfigurationManager.AppSettings["Smt:TimerIntervalInSeconds"]);

                string[] AssemblyReader = ConfigurationManager.AppSettings["Smt:AssemblyReader"].Split(',');

                string[] MarketPlaceReader = ConfigurationManager.AppSettings["Smt:MarketPlaceReader"].Split(',');

                string[] AcceptThisUnknownGateway = MarketPlaceReader.Concat(AssemblyReader).ToArray();

                string[] QualityReader = ConfigurationManager.AppSettings["Smt:QualityReader"].Split(','); ;



                var MissingTag = "UPDATE mSMTProduct SET Status='Missing From Assembly', ModifiedBy='Job' where Ble = @p0";

                var MarkMissing = "UPDATE toMonitor SET LastSeenTime = GETDATE() where Epc = @p0";

                var TotalMissingFromAssebly = "Insert into toTrackInfo(RFID,UID,tDate,AppUserName)Values(@p0,@p1,GETDATE(),@p2)";

                var AtAssembly = $"UPDATE mSMTProduct SET IsAssembly = 1, Ble = NULL, Status = 'At assembly',ModifiedBy='Job' where Ble = @p0";

                var AtUnknownZone = $"UPDATE mSMTProduct SET Status=@p0, ModifiedBy=@p1, ModifiedDate=GETDATE() where Ble=@p2";

                var _Live = $"UPDATE toMonitor SET LastSeenTime = NULL where Epc = @p2);";

                var AddAlertAtUnknownZone = "Insert into toTrackInfo(RFID,UID,tDate,AppUserName)Values(@p3,@4,GETDATE(),@p5)";

                var NormalReturn = "Insert into toTrackInfo(RFID,UID,tDate,AppUserName)Values(@p0,@1,GETDATE(),@p2)";

                var QualityReturn = "Insert into toTrackInfo(RFID,UID,tDate,AppUserName)Values(@p0,@1,GETDATE(),@p2)";

                //var parameters = new List<SqlParameter>();

                using (var contx = new ERPdbEntities())
                {
                    var y = (from pro in contx.mSMTProducts
                             join ble in contx.toMonitors
                             on pro.Ble equals ble.Epc
                             join p in contx.mReaderSettups on ble.Name equals p.ReaderNo into r
                             from _r in r.DefaultIfEmpty()
                             join z in contx.mZones on _r.mZoneId equals z.mZoneId into z
                             from _z in z.DefaultIfEmpty()
                             where ble.Id != null
                             select new
                             {
                                 Monitor = ble,
                                 Product = pro,
                                 Zone = _z
                             });

                    var MissingFromAssembly = y.Where(reader => _AssemblyReader.Contains(reader.Monitor.Name))
                         .Where(tm => DbFunctions.DiffSeconds(tm.Monitor.tDate, dateLimit) > _TimerIntervalInSeconds)
                         .Where(p => p.Product.IsTakeaway == true && p.Product.IsAssembly == false && p.Product.IsAction == true);

                    foreach (var u in MissingFromAssembly.ToList())
                        if (u.Monitor.LastSeenTime == null)
                            contx.Database.ExecuteSqlCommand($"{MissingTag}; {MarkMissing}; {TotalMissingFromAssebly}", u.Product.Ble, MissingFromAssembly.Count(), System.Guid.NewGuid().ToString());
                        else
                            contx.Database.ExecuteSqlCommand($"{AtAssembly}", u.Product.Ble);

                    var UnknownZone = y.Where(reader => AcceptThisUnknownGateway.Contains(reader.Monitor.Name));
                    foreach (var e in UnknownZone.ToList())
                        if (!string.IsNullOrWhiteSpace(e.Zone.Zone))
                            contx.Database.ExecuteSqlCommand($"{AtUnknownZone}; {AddAlertAtUnknownZone}; {_Live}",
                                e.Zone.Zone,
                                e.Monitor.Name,
                                e.Product.Ble,
                                $"{e.Monitor.Epc} is in Unknown location : {e.Zone.Zone}",
                                AlertType.Alert,
                                System.Guid.NewGuid().ToString());

                    var NormalReturnBin = y.Where(x => !MarketPlaceReader.Contains(x.Monitor.Name) && DbFunctions.DiffMinutes(x.Monitor.tDate, dateLimit) > 180)
                        .Where(b => b.Product.Status == JobStatus.ReturnRequestApproved);
                    foreach (var r in NormalReturnBin.ToList())
                        contx.Database.ExecuteSqlCommand($"{NormalReturn}",
                            $"{r.Monitor.Epc} Return BIN not reached Marketplace since {r.Monitor.tDate}", AlertType.Alert, System.Guid.NewGuid().ToString());


                    var QualityReturnBin = y.Where(x => !QualityReader.Contains(x.Monitor.Name) && DbFunctions.DiffMinutes(x.Monitor.tDate, dateLimit) > 180)
                        .Where(b => b.Product.Status == JobStatus.QualityReturnRequestApproved);
                    foreach (var q in QualityReturnBin.ToList())
                        contx.Database.ExecuteSqlCommand($"{QualityReturn}",
                            $"{q.Monitor.Epc} Quality Return not reached since {q.Monitor.tDate}", AlertType.Alert, System.Guid.NewGuid().ToString());


                    //var demo = y.Where(x => !QualityReader.Contains(x.Monitor.Name) && DbFunctions.DiffMinutes(x.Monitor.tDate, dateLimit) > 180)
                    //    .ExecuteUpdate();
                }

            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
                //return false;
            }

        }

        public void MasterTrigger()
        {
            try
            {
                var t = Task.Run(() => TiggerAlertJob());
                t.Wait();
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
                //return false;
            }
        }
        public void BinNotOrReachesAssembly()
        {
            try
            {
                var _reader = ConfigurationManager.AppSettings["Smt:AssemblyReader"].Split(',');
                var _TimerIntervalInSeconds = Convert.ToDouble(ConfigurationManager.AppSettings["Smt:TimerIntervalInSeconds"]);
                var dateLimit = DateTime.Now;
                using (var contx = new ERPdbEntities())
                {
                    try
                    {
                        contx.Database.ExecuteSqlCommand("DISABLE TRIGGER ALL ON mSMTProduct");
                        contx.BulkSaveChanges(options => options.BatchTimeout = 180);

                        var filter = contx.toMonitors.Where(r => _reader.Contains(r.Name)).AsEnumerable().AsQueryable();
                        if (filter.Count() > 0)
                        {
                            var tAsseblyMissing = (from f in filter.Where(tm => DbFunctions.DiffSeconds(tm.tDate, dateLimit) > _TimerIntervalInSeconds)
                                                   join p in contx.mSMTProducts on f.Epc equals p.Ble
                                                   where p.IsTakeaway == true && p.IsAssembly == false && p.IsAction == true && f.LastSeenTime == null
                                                   select new { p, f }).ToList();

                            if (tAsseblyMissing.Count() > 0)
                            {
                                var i = tAsseblyMissing.Select(x => string.Format("{0}", x.p.Ble)).ToList();

                                var _bleList = string.Join(",", tAsseblyMissing.Select(x => string.Format("'{0}'", x.p.Ble)));
                                var _updateAssebly = $"UPDATE mSMTProduct SET Status='Missing From Assembly',ModifiedBy='Job' where Ble In({_bleList});";
                                contx.Database.ExecuteSqlCommand(_updateAssebly);

                                //var entitiesToUpdate = contx.mSMTProducts.Where(e => i.Contains(e.Ble));
                                //entitiesToUpdate.Update(x => new mSMTProduct { Status = "Missing From Assembly", ModifiedBy = "Job", ModifiedDate = DateTime.Now });
                                // contx.SaveChanges();

                                var _deleteMonitor = $"UPDATE toMonitor SET LastSeenTime = GETDATE() where Epc in({_bleList});";
                                contx.Database.ExecuteSqlCommand(_deleteMonitor);

                                //var entitiesToMonitor = contx.toMonitors.Where(e => i.Contains(e.Epc));
                                //entitiesToMonitor.Update(x => new toMonitor { LastSeenTime = DateTime.Now.ToString() });
                                //contx.SaveChanges();


                                var t = new toTrackInfo
                                {
                                    RFID = $"{tAsseblyMissing.Count()} count of BINS are not reached Assembly area.",
                                    UID = AlertType.Alert,
                                    tDate = DateTime.Now,
                                    AppUserName = System.Guid.NewGuid().ToString()
                                };

                                contx.toTrackInfoes.Add(t);
                                //ontx.SaveChanges();


                            }

                            var tMarkAssembly = (from f in filter
                                                 join p in contx.mSMTProducts on f.Epc equals p.Ble
                                                 where p.IsTakeaway == true && p.IsAssembly == false && p.IsAction == true
                                                 select p).ToList();

                            if (tMarkAssembly.Count > 0)
                            {

                                var i = tMarkAssembly.Select(x => string.Format("{0}", x.Ble)).ToList();
                                var entitiesToUpdate = contx.mSMTProducts.Where(e => i.Contains(e.Ble));
                                entitiesToUpdate.Update(x => new mSMTProduct { IsAssembly = true, Ble = null, Status = "At assembly", ModifiedBy = "Job", ModifiedDate = DateTime.Now });


                                //var _bleLists = string.Join(",", tMarkAssembly.Select(x => string.Format("'{0}'", x.Ble)));
                                //var _updateAssebly = $"UPDATE mSMTProduct SET IsAssembly = 1, Ble = NULL, Status = 'At assembly',ModifiedBy='Job' where Ble In({_bleLists});";
                                //contx.Database.ExecuteSqlCommand(_updateAssebly);
                                //
                                //var _DeleteAlert = $"DELETE FROM toTrackInfo where RFID In({_bleLists});";
                                //contx.Database.ExecuteSqlCommand(_updateAssebly);
                            }

                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        contx.SaveChanges();
                        contx.Database.ExecuteSqlCommand("ENABLE TRIGGER ALL ON mSMTProduct");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
            }
        }

        public void UnknownGateway()
        {
            try
            {
                var asm = ConfigurationManager.AppSettings["Smt:AssemblyReader"].Split(',');

                string[] keys = new string[] { ConfigurationManager.AppSettings["Smt:MarketPlaceReader"] };

                string[] mergedArray = keys.Concat(asm).ToArray();

                using (var contx = new ERPdbEntities())
                {
                    contx.Database.ExecuteSqlCommand("DISABLE TRIGGER ALL ON mSMTProduct");

                    var y = (from f in contx.toMonitors.Where(x => !mergedArray.Contains(x.Name))
                             join p in contx.mReaderSettups on f.Name equals p.ReaderNo into r
                             from _r in r.DefaultIfEmpty()
                             join z in contx.mZones on _r.mZoneId equals z.mZoneId into z
                             from _z in z.DefaultIfEmpty()
                             select new
                             {
                                 f.Name,
                                 f.Epc,
                                 _z.Zone
                             }).AsEnumerable().AsQueryable();

                    if (y.Count() > 0)
                    {
                        foreach (var e in y.ToArray())
                        {
                            if (!string.IsNullOrEmpty(e.Zone))
                            {
                                var _updateUnknown = $"UPDATE mSMTProduct SET Status='{e.Zone}', ModifiedBy='{e.Name}', ModifiedDate=GETDATE() where Ble='{e.Epc}';";
                                contx.Database.ExecuteSqlCommand(_updateUnknown);

                                try
                                {
                                    var t = new toTrackInfo
                                    {
                                        RFID = $"{e.Epc} is in Unknown location : {e.Zone}",
                                        UID = AlertType.Alert,
                                        tDate = DateTime.Now,
                                        AppUserName = System.Guid.NewGuid().ToString()
                                    };

                                    contx.toTrackInfoes.Add(t);
                                    contx.SaveChanges();

                                }
                                catch (Exception ex)
                                {
                                    WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
                                }
                            }
                        }
                        try
                        {
                            var _bleUnknown = string.Join(",", y.Select(x => string.Format("'{0}'", x.Epc)));
                            var _bleSql = $"UPDATE toMonitor SET LastSeenTime = NULL where Epc in({_bleUnknown});";
                            contx.Database.ExecuteSqlCommand(_bleSql);
                        }
                        catch (Exception ex)
                        {
                            WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
                        }
                    }

                    contx.Database.ExecuteSqlCommand("ENABLE TRIGGER ALL ON mSMTProduct");

                }
            }
            catch (Exception) { }
        }

        public void ReturnBIN()
        {
            try
            {
                string[] keys = ConfigurationManager.AppSettings["Smt:MarketPlaceReader"].Split(',');

                var dateLimit = DateTime.Now;

                using (var contx = new ERPdbEntities())
                {
                    contx.Database.ExecuteSqlCommand("DISABLE TRIGGER ALL ON mSMTProduct");

                    var y = (from f in contx.toMonitors.Where(x => !keys.Contains(x.Name) && DbFunctions.DiffMinutes(x.tDate, dateLimit) > 180)
                             join pd in contx.mSMTProducts on f.Epc equals pd.Ble into ble
                             from _ble in ble.DefaultIfEmpty()
                             join p in contx.mReaderSettups on f.Name equals p.ReaderNo into r
                             from _r in r.DefaultIfEmpty()
                             join z in contx.mZones on _r.mZoneId equals z.mZoneId into z
                             from _z in z.DefaultIfEmpty()
                             where _ble.Status == JobStatus.ReturnRequestApproved
                             select new
                             {
                                 f.Name,
                                 f.Epc,
                                 f.tDate,
                                 _z.Zone
                             }).AsEnumerable().AsQueryable();

                    if (y.Count() > 0)
                    {
                        Task.Run(() =>
                        {
                            foreach (var i in y.ToArray())
                            {
                                var t = new toTrackInfo
                                {
                                    RFID = $"{i.Epc} Return BIN not reached Marketplace since {i.tDate}",
                                    UID = AlertType.Alert,
                                    tDate = DateTime.Now,
                                    AppUserName = System.Guid.NewGuid().ToString()
                                };

                                contx.toTrackInfoes.Add(t);
                                contx.SaveChanges();
                            }
                        });
                    }
                    contx.Database.ExecuteSqlCommand("ENABLE TRIGGER ALL ON mSMTProduct");

                }
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
            }
        }

        public void QualityReturn()
        {
            try
            {
                string[] keys = new string[] { ConfigurationManager.AppSettings["Smt:QualityReader"] };

                var dateLimit = DateTime.Now;

                using (var contx = new ERPdbEntities())
                {
                    contx.Database.ExecuteSqlCommand("DISABLE TRIGGER ALL ON mSMTProduct");

                    var y = (from f in contx.toMonitors.Where(x => !keys.Contains(x.Name) && DbFunctions.DiffMinutes(x.tDate, dateLimit) > 900)
                             join pd in contx.mSMTProducts on f.Epc equals pd.Ble into ble
                             from _ble in ble.DefaultIfEmpty()
                             join p in contx.mReaderSettups on f.Name equals p.ReaderNo into r
                             from _r in r.DefaultIfEmpty()
                             join z in contx.mZones on _r.mZoneId equals z.mZoneId into z
                             from _z in z.DefaultIfEmpty()
                             where _ble.Status == JobStatus.QualityReturnRequestApproved
                             select new
                             {
                                 f.Name,
                                 f.Epc,
                                 f.tDate,
                                 _z.Zone
                             }).AsEnumerable().AsQueryable();

                    if (y.Count() > 0)
                    {
                        Task.Run(() =>
                        {
                            foreach (var i in y.ToArray())
                            {
                                var t = new toTrackInfo
                                {
                                    RFID = $"{i.Epc} Quality Return not reached since {i.tDate}",
                                    UID = AlertType.Alert,
                                    tDate = DateTime.Now,
                                    AppUserName = System.Guid.NewGuid().ToString()
                                };
                                using (var c = new ERPdbEntities())
                                {
                                    c.toTrackInfoes.Add(t);
                                    c.SaveChanges();
                                }
                            }
                        });
                    }
                    contx.Database.ExecuteSqlCommand("ENABLE TRIGGER ALL ON mSMTProduct");

                }
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorLog($"{MethodBase.GetCurrentMethod().DeclaringType.FullName} :{DateTime.Now} Exception :{ex.Message} |  InnerException :{ex.InnerException?.Message}");
            }
        }

        static void MissingAsset(List<mSMTProduct> ticket)
        {
            try
            {

                //String text, strSubject;
                //strSubject = "Asset Missing";
                //StringBuilder strBody = new StringBuilder();

                //strBody.Append("<html xmlns=\"http://www.w3.org/1999/xhtml/\">");
                //strBody.Append("<head><title>" + strSubject + "</title><style>td{font-weight:bold;} </style></head>");
                //strBody.Append("<body style=\"font-size: 12pt; font-family: Courier New;\">");
                //strBody.Append("<br /><font face=\"Courier New\" size=\"3\">ITAM Notification</font><br /><br />");
                //strBody.Append("<table border='1' width=\"100%\" align=\"center\" style=\"font-family: Courier New; -webkit-font-smoothing: antialiased;font-size: 12px;overflow: auto;text-align: left; border: 2px solid Gray;\">");
                //strBody.Append("<tr style='background-color: #99ccff;");
                //strBody.Append(" color: black;padding: 6px 10px;font-weight: bold; border-right-color: Black;border-right-width: 1px;'>");
                //strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;PartNumber</font></p>");
                //strBody.Append("</td>");
                //strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;SerialNumber</font></p>");
                //strBody.Append("</td>");
                //strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;Ble</font></p>");
                //strBody.Append("</td>");
                //strBody.Append("<td><p><font face=\"Courier New\" size=\"2\">&nbsp;&nbsp;Last Seen</font></p>");
                //strBody.Append("</td>");
                //strBody.Append("</tr>");
                //foreach (var i in ticket)
                //{
                //    strBody.Append("<tr style='height: 25px;'>");
                //    strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + i.PartNumber + "</td>");
                //    strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + i.SerialNumber + "</td>");
                //    strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + i.Ble + "</td>");
                //    strBody.Append("<td style='padding: 5px 10px 5px 5px;'>" + DateTime.Now + "</td>");
                //    strBody.Append("</tr>");
                //}
                //strBody.Append("</table><br /><br />");
                //strBody.Append("</body></html>");

                //MailMessage message = new MailMessage();
                //message.From = new MailAddress(ConfigurationManager.AppSettings["EmailId"].ToString());
                //string[] missingTo = ConfigurationManager.AppSettings["missingTo"].Split(';');
                //string[] missingCC = ConfigurationManager.AppSettings["missingCC"].Split(';');

                //if (missingCC.Length > 0)
                //{
                //    foreach (var _eid in missingCC)
                //    {
                //        message.CC.Add(new MailAddress(_eid));
                //    }
                //}
                //if (missingTo.Length > 0)
                //{
                //    foreach (var _eid in missingTo)
                //    {
                //        message.To.Add(new MailAddress(_eid));
                //    }
                //}

                ////string htmlBody;
                ////htmlBody = strBody;
                //string returnUrl = strBody.ToString();
                //message.Body = returnUrl;
                //message.Subject = "Asset Missing Alert";
                //message.IsBodyHtml = true;
                //message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                /////smtp server and port configured at registry
                //SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["EmailServer"], Convert.ToInt32(ConfigurationManager.AppSettings["Port"]));
                /////enable ssl is required for secure connection.It is must be true for gmail server and false for other servers.
                //smtpClient.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSSL"]);
                //smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtpClient.UseDefaultCredentials = true;
                ////smtpClient.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["EmailId"], WebConfigurationManager.AppSettings["Password"]);
                //smtpClient.Send(message);

                ////NotifyMail.FireAndForgetTaskAsync(async () =>
                ////{
                ////});

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0} : {1}", "MissingAsset", ex.Message);
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }
        }
    }
}
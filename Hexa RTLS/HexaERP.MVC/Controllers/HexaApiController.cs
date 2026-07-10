using HexaERP.MVC.Hubs;
using HexaERP.MVC.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace HexaERP.MVC.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HexaApiController : ApiController
    {
        private static IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<GetTags>();

        private static IHubContext _APIStatusHub = GlobalHost.ConnectionManager.GetHubContext<APIStatusHub>();

        private ERPdbEntities db = new ERPdbEntities();

        //
        //static List<tToolTrackDemo> tagsTrack = new List<tToolTrackDemo>();
        static Dictionary<string, Int32> tagsRead = new Dictionary<string, Int32>();

        private static Dictionary<string, decimal> threshold = new Dictionary<string, decimal>();

        // GET: api/HexaApi/GetReaders      
        private static double RssiVal =
            Convert.ToDouble(WebConfigurationManager.AppSettings["RssiVal"]);
        public static bool flag = false; String EpcKey; decimal tryVal;

        public static void Callback(object state)
        {
            try
            {
                if (tagsRead.Count > 0)
                {
                    tagsRead.Clear();
                    // tagsTrack.Clear();
                    //_APIStatusHub.Clients.All.getAPIStatus(DateTime.Now.ToShortTimeString(), true);
                }
                else
                {
                    //_APIStatusHub.Clients.All.getAPIStatus("Error : Data Not Filttering", false);
                }
            }
            catch (Exception ex)
            {
                // _APIStatusHub.Clients.All.getAPIStatus("Error Callback :" + ex.Message, false);
            }
        }

        public static void thresholdlevels()
        {
            try
            {
                threshold.Clear();
                if (threshold.Count() <= 0)
                {
                    threshold.Add("30001", 90);
                    threshold.Add("30002", 75);
                    threshold.Add("30003", 80);
                    threshold.Add("30004", 87);
                    threshold.Add("30005", 90);
                    threshold.Add("30006", 100);
                    threshold.Add("30007", 88);
                    threshold.Add("30009", 95);

                    //threshold.Add("30001", 90);
                    //threshold.Add("30002", 90);
                    //threshold.Add("30003", 90);
                    //threshold.Add("30004", 90);
                    //threshold.Add("30005", 90);
                    //threshold.Add("30006", 90);
                    //threshold.Add("30007", 90);
                    //threshold.Add("30009", 90);
                    flag = true;
                    _APIStatusHub.Clients.All.getAPIStatus("Sucess : threshold levels data loaded", true);
                }
                else
                {
                    _APIStatusHub.Clients.All.getAPIStatus("Error : threshold levels data not loaded", false);
                }
            }
            catch (Exception ex)
            {
                _APIStatusHub.Clients.All.getAPIStatus("Error threshold levels:" + ex.Message, false);
            }
        }


        public IHttpActionResult Get()
        {

            _APIStatusHub.Clients.All.getAPIStatus("Time {0}", DateTime.Now);
            return Ok(DateTime.Now);
        }

        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IHttpActionResult> GetReaders()
        {
            int OrgId = 1037;
            var result = await db.mReaderSettups.Where(j => j.IsAction == true && j.OrgInfoId == OrgId).Select(c => new { c.ReaderIP, c.ReaderNo }).Distinct().ToListAsync();
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [ApiExplorerSettings(IgnoreApi = false)]
        //Insert RFID Logs  
        [HttpPost]
        [AllowAnonymous]
        [Route("api/TrackLogOld")]
        public HttpResponseMessage TrackLogOld(toTrackInfo obj)
        {
            try
            {
                db.toTrackInfoes.Add(obj);
                db.SaveChanges();
                return new HttpResponseMessage()
                {
                    Content = new StringContent("PUT: Successfull")
                };
            }
            catch (Exception ex)
            {
                string[] errorlines = { "Method:" + "Track Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);

                FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();

                return new HttpResponseMessage()
                {
                    Content = new StringContent("PUT: Error")
                };
            }
        }

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost]
        [AllowAnonymous]
        [Route("api/TrackLog")]
        public HttpResponseMessage TrackLog(toTrackInfo obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(obj.RFID))
                {

                    EpcKey = obj.RFID.ToString() + obj.ReaderNo;

                    if (!tagsRead.ContainsKey(EpcKey))
                    {
                        if (flag)
                        {
                            if (threshold.TryGetValue(obj.ReaderIP, out tryVal))
                            {
                                decimal _rssi = Math.Abs(Convert.ToDecimal(obj.RSSI));

                                if (_rssi <= tryVal)
                                {
                                    tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                                    db.toTrackInfoes.Add(obj);
                                    db.SaveChanges();
                                }
                                // item with key exists so you can use the tryVal
                            }

                            //double Rssi = Math.Abs(Convert.ToDouble(obj.RSSI));
                            //if (Rssi <= 90)
                            //{
                            //    tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                            //    db.toTrackInfoes.Add(obj);
                            //    db.SaveChanges();
                            //}

                            // var pair = threshold.FirstOrDefault(p => p.Key == obj.ReaderNo.ToString());


                            //if (threshold.ContainsKey(obj.ReaderNo.ToString()))
                            //{
                            //    double Rssi = Math.Abs(Convert.ToDouble(obj.RSSI));
                            //    double value = threshold[obj.ReaderNo.ToString()];
                            //    if (Rssi <= value)
                            //    {
                            //        tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                            //        db.toTrackInfoes.Add(obj);
                            //        db.SaveChanges();
                            //    }
                            //}
                        }

                        //try
                        //{
                        //}
                        //catch (Exception ex) { _APIStatusHub.Clients.All.getAPIStatus("Error threshold Key:" + ex.Message, true); }
                        //tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                        ////obj.RSSI = Convert.ToDecimal(Rssi);
                        //db.toTrackInfoes.Add(obj);
                        //db.SaveChanges(); 
                        //double Rssi = Math.Abs(Convert.ToDouble(obj.RSSI));
                        //double value = threshold[obj.ReaderNo];
                        //if (Rssi >= 45)
                        //{
                        //    tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                        //    //obj.RSSI = Convert.ToDecimal(Rssi);
                        //    db.toTrackInfoes.Add(obj);
                        //    db.SaveChanges();
                        //    //if (Rssi <= RssiVal)
                        //    //{
                        //    //}
                        //    //    threshold.ToList().ForEach(uq =>
                        //    //{
                        //    //});
                        //}
                        //threshold.FirstOrDefault(t => t.Key == obj.ReaderNo);


                    }
                }
                else
                {
                    // _APIStatusHub.Clients.All.getAPIStatus("Json Empty: Error:", true);
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("Json Empty: Error")
                    };
                }

            }

            //catch (DbEntityValidationException ex)
            //{
            //    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            //    {
            //        // Get entry
            //        DbEntityEntry entry = item.Entry;
            //        string entityTypeName = entry.Entity.GetType().Name;
            //        // Display or log error messages
            //        foreach (DbValidationError subItem in item.ValidationErrors)
            //        {
            //            //_APIStatusHub.Clients.All.getAPIStatus("Error : Data Not Filttering", false);
            //            string message = string.Format("Error '{0}' occurred in {1} at {2}",
            //                     subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
            //            _APIStatusHub.Clients.All.getAPIStatus(subItem.ErrorMessage + entityTypeName + subItem.PropertyName, true);
            //            string[] errorlines = {string.Format("Error '{0}' occurred in {1} at {2}",
            //                     subItem.ErrorMessage, entityTypeName, subItem.PropertyName)};
            //            //Console.WriteLine(message);
            //            File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);

            //            FileStream fs = new FileStream(@"C:\Windows\Temp\ErrorLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
            //            StreamWriter sw = new StreamWriter(fs);
            //            sw.BaseStream.Seek(0, SeekOrigin.End);
            //            sw.WriteLine(message);
            //            sw.Flush();
            //            sw.Close();
            //        }
            //    }
            //}
            catch (Exception ex)
            {
                _APIStatusHub.Clients.All.getAPIStatus(ex.Message, true);

                string[] errorlines = { "Method:" + "Track Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ExceptionLog.txt", errorlines);

                FileStream fs = new FileStream(@"C:\Windows\Temp\ExceptionLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();

                return new HttpResponseMessage()
                {
                    Content = new StringContent("Exception: " + ex.Message)
                };

            }

            return new HttpResponseMessage()
            {
                Content = new StringContent("Sucessfully : ")
            };

        }



        // [HttpPost]
        // [AllowAnonymous]
        // [Route("api/TrackLog")]
        // public HttpResponseMessage TrackLog(List<toTrackInfo> formdata)
        // {
        //     try
        //     {
        //         if (formdata.Count > 0)
        //         {
        //             formdata.ToList().ForEach(
        //obj =>
        //{
        //    String EpcKey;
        //    EpcKey = obj.RFID.ToString() + "-" + obj.ReaderNo + "-" + obj.mAttPortId.ToString();
        //    //double Rssi = Math.Abs(RssiVal);
        //    if (!tagsRead.ContainsKey(EpcKey))
        //    {
        //        // Add this tag to the list of tags we've read.
        //        // InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber, sender.Name, sender.Address);
        //        tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
        //        db.toTrackInfoes.Add(obj);
        //        db.SaveChanges();

        //        //if (Rssi <= RssiVal)
        //        //{

        //        //}
        //    }

        //});
        //             //String Key; string PortId;

        //         }
        //         else
        //         {
        //             return new HttpResponseMessage()
        //             {
        //                 Content = new StringContent("Json Empty: Error")
        //             };


        //         }

        //     }

        //     catch (Exception ex)
        //     {
        //         string[] errorlines = { "Method:" + "Track Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
        //         //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
        //         File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);

        //         FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
        //         StreamWriter sw = new StreamWriter(fs);
        //         sw.BaseStream.Seek(0, SeekOrigin.End);
        //         sw.WriteLine(errorlines);
        //         sw.Flush();
        //         sw.Close();

        //         return new HttpResponseMessage()
        //         {
        //             Content = new StringContent("Exception: " + ex.Message)
        //         };

        //     }

        //     return new HttpResponseMessage()
        //     {
        //         Content = new StringContent("Error In API : Error")
        //     };

        // }
        [ApiExplorerSettings(IgnoreApi = false)]
        //Insert RFID Logs
        [HttpPost]
        [AllowAnonymous]
        [Route("api/AntennaLog")]
        public IHttpActionResult AntennaLog(tPortChangeLog obj)
        {
            try
            {
                db.tPortChangeLogs.Add(obj);
                db.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                string[] errorlines = { "Method:" + "Antenna Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);

                FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();

                return BadRequest();
            }
        }
        [ApiExplorerSettings(IgnoreApi = false)]
        //Insert Reader Status Logs
        [HttpPost]
        [AllowAnonymous]
        [Route("api/ReaderLog")]
        public IHttpActionResult ReaderLog(tReaderLog obj)
        {
            try
            {
                db.tReaderLogs.Add(obj);
                db.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                string[] errorlines = { "Method:" + "Reader Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);


                FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();

                return BadRequest();
            }
        }
        [ApiExplorerSettings(IgnoreApi = false)]
        //Insert Reader Status Logs
        [HttpPost]
        [AllowAnonymous]
        [Route("api/MonitorLog")]
        public async Task<IHttpActionResult> MonitorLog(List<tToolTrackDemo> obj)
        {
            try
            {


                var newJson = JsonConvert.SerializeObject(obj);
                // File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + ("Content/EmployeeTag.json"), newJson);
                await Task.Run(() => File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + ("Content/EmployeeTag.json"), newJson));
                //File.Crete(AppDomain.CurrentDomain.BaseDirectory("~/Content/EmployeeTag.json"), newJson);      
                return Ok();
            }
            catch (Exception ex)
            {
                string[] errorlines = { "Method:" + "Monitor Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);


                FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();
                return BadRequest();
            }
        }

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost]
        [Route("api/JsonTrackLog")]
        public IHttpActionResult JsonTrackLog([FromBody] toTrackInfo obj)
        {

            var _message = string.Format("Not Peocessed Data");
            HttpError err = new HttpError(_message);

            try
            {
                //String Key; string PortId;
                String EpcKey;
                EpcKey = obj.RFID.ToString() + "-" + obj.ReaderNo + "-" + obj.mAttPortId.ToString();

                //double Rssi = Math.Abs(tag.PeakRssiInDbm);

                if (!tagsRead.ContainsKey(EpcKey))
                {
                    // Add this tag to the list of tags we've read.
                    // InsertTrack(tag.Epc.ToString(), tag.AntennaPortNumber, sender.Name, sender.Address);
                    tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                    db.toTrackInfoes.Add(obj);
                    db.SaveChanges();
                    var message = string.Format("Record Inserted = {0}" + obj.RFID);
                    return ResponseMessage(
          Request.CreateResponse(
              HttpStatusCode.OK,
              message));

                    //if (Rssi <= RssiVal)
                    //{                       
                    //}

                }

            }
            catch (Exception ex)
            {
                string[] errorlines = { "Method:" + "Track Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);

                FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();
                return ResponseMessage(
        Request.CreateResponse(
            HttpStatusCode.InternalServerError,
            ex.Message));

            }

            return ResponseMessage(
         Request.CreateResponse(
             HttpStatusCode.NotFound,
             err));

        }
        [ApiExplorerSettings(IgnoreApi = false)]
        //Insert Reader        
        [HttpPost]
        [AllowAnonymous]
        [Route("api/HexaApi/DeskTopReader")]
        public IHttpActionResult DeskTopReader(DesltopEntity obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(obj.RFID))
                {
                    _hubContext.Clients.All.getrfid(obj.RFID, obj.RFID);
                    return Ok();
                }
                else { return BadRequest(); }

            }
            catch (Exception ex)
            {
                string[] errorlines = { "Method:" + "Reader Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.InnerException.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);
                FileStream fs = new FileStream(@"C:\Windows\Temp\ServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();

                return BadRequest();
            }
        }


        public class DesltopEntity
        {
            public string RFID { get; set; }
        }

        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost]
        [AllowAnonymous]
        [Route("api/DataLog")]
        public HttpResponseMessage DataLog(toTrackInfo obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(obj.RFID))
                {
                    EpcKey = obj.RFID.ToString() + obj.ReaderNo + obj.mAttPortId;
                    if (!tagsRead.ContainsKey(EpcKey))
                    {
                        if (flag)
                        {

                            tagsRead.Add(EpcKey, Convert.ToInt32(obj.mAttPortId));
                            db.toTrackInfoes.Add(obj);
                            db.SaveChanges();
                            //decimal _rssi = Math.Abs(Convert.ToDecimal(obj.RSSI));
                            //if (_rssi <= tryVal)
                            //{                                  
                            //}
                            // item with key exists so you can use the tryVal                          

                        }

                    }
                }
                else
                {
                    // _APIStatusHub.Clients.All.getAPIStatus("Json Empty: Error:", true);
                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("Json Empty: Error")
                    };
                }
            }

            //catch (DbEntityValidationException ex)
            //{
            //    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            //    {
            //        // Get entry
            //        DbEntityEntry entry = item.Entry;
            //        string entityTypeName = entry.Entity.GetType().Name;
            //        // Display or log error messages
            //        foreach (DbValidationError subItem in item.ValidationErrors)
            //        {
            //            //_APIStatusHub.Clients.All.getAPIStatus("Error : Data Not Filttering", false);
            //            string message = string.Format("Error '{0}' occurred in {1} at {2}",
            //                     subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
            //            _APIStatusHub.Clients.All.getAPIStatus(subItem.ErrorMessage + entityTypeName + subItem.PropertyName, true);
            //            string[] errorlines = {string.Format("Error '{0}' occurred in {1} at {2}",
            //                     subItem.ErrorMessage, entityTypeName, subItem.PropertyName)};
            //            //Console.WriteLine(message);
            //            File.AppendAllLines(@"C:\Windows\Temp\ErrorLog.txt", errorlines);

            //            FileStream fs = new FileStream(@"C:\Windows\Temp\ErrorLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
            //            StreamWriter sw = new StreamWriter(fs);
            //            sw.BaseStream.Seek(0, SeekOrigin.End);
            //            sw.WriteLine(message);
            //            sw.Flush();
            //            sw.Close();
            //        }
            //    }
            //}
            catch (Exception ex)
            {
                _APIStatusHub.Clients.All.getAPIStatus(ex.Message, true);

                string[] errorlines = { "Method:" + "Track Log", "Date :" + Convert.ToString(DateTime.Now), "Error :" + ex.Message + Environment.NewLine };
                //string mydocpath = AppDomain.CurrentDomain.BaseDirectory;
                File.AppendAllLines(@"C:\Windows\Temp\ExceptionLog.txt", errorlines);

                FileStream fs = new FileStream(@"C:\Windows\Temp\ExceptionLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(errorlines);
                sw.Flush();
                sw.Close();

                return new HttpResponseMessage()
                {
                    Content = new StringContent("Exception: " + ex.Message)
                };

            }

            return new HttpResponseMessage()
            {
                Content = new StringContent("Sucessfully : ")
            };

        }

    }
}

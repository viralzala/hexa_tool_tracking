using HexaERP.MVC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HexaERP.MVC.Controllers.CRM
{
    public class LeadBulkUploadController : Controller
    {
        //Author: Mudassar I
        private ERPdbEntities db = new ERPdbEntities();
        // GET: LeadBulkUpload
        public ActionResult Index()
        {
            return View();
        }
        //Craete Lead Contact
        [HttpGet]
        public int Save(string Json)
        {
            int Msg = 0;
            try
            {
                var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
                DataTable dt = new DataTable();
                dt = JsonStringToDataTable(Json);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; dt.Rows.Count > i; i++)
                    {
                        string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("x");
                        //DeSerialize
                        //var obj = JsonConvert.DeserializeObject<LeadEntity>(Json);
                        string COntacts = dt.Rows[i].ItemArray.GetValue(4).ToString();
                        using (var context = new ERPdbEntities())
                        {
                            var ObData = context.TC_Customer
                                            .Where(b => b.Contact == COntacts)
                                             .Select(p => new { p.TC_CustomerId, p.Uid }).ToList();
                            //var FoundId = ObData.TC_CustomerId;                    
                            if (ObData.Any())
                            {
                                var FoundId = ObData.Single();
                                //Msg = "Contact Number Alredy Exist";
                            }
                            else
                            {
                                var cFlag = context.Database.ExecuteSqlCommand(
                                      "INSERT INTO TC_Customer(Uid,Name,LastName,Gender,EmailId,Contact,CompanyName,Designation,Address,City,State,PinCode)" +
                                      "VALUES('" + Uid + "','" + dt.Rows[i].ItemArray.GetValue(0).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(1).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(2).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(3).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(4).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(5).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(6).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(7).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(8).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(9).ToString() + "','" + dt.Rows[i].ItemArray.GetValue(10).ToString() + "')");
                                if (cFlag == 1)
                                {
                                    //Get
                                    var cData = context.TC_Customer
                                                    .Where(b => b.Uid == Uid)
                                                     .Select(p => new { p.TC_CustomerId, p.Uid }).ToList();
                                    var cFoundId = cData.Single();

                                    var lFlag = context.Database.ExecuteSqlCommand(
                                      "INSERT INTO TC_Lead(CustomerId,LeadCreationDate,CreatedDate,CreatedBy,OrgInfoID) " +
                                      "VALUES('" + cFoundId.TC_CustomerId + "',GETDATE(),GETDATE(),'" + UserName + "','" + orgId + "')");
                                    db.SaveChanges();
                                    Msg = Msg + 1;
                                }
                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                string err = ex.Message.ToString();
                // Convert.ToString(Msg = err.ToString());
            }

            return Msg;
        }
        [HttpPost]
        // use IEnumerable<HttpPostedFileBase> files if you want to post multiple files
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/UploadFile/"), fileName);
                file.SaveAs(path);
            }
            return RedirectToAction("Index");
        }
        //
        [HttpPost]
        public JsonResult UploadLeads()
        {
            try
            {
                if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var pic = System.Web.HttpContext.Current.Request.Files["HelpSectionImages"];
                    HttpPostedFileBase filebase = new HttpPostedFileWrapper(pic);
                    var fileName = Path.GetFileName(filebase.FileName);
                    var Ext = Path.GetExtension(filebase.FileName);
                    var Paths = Path.GetPathRoot(filebase.FileName);

                    //var path = Path.Combine(Server.MapPath("~/UploadFile/"), fileName);
                    var path = (filebase.InputStream);
                    //filebase.SaveAs(path);
                    DataTable dt = new DataTable();
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string[] headers = sr.ReadLine().Split(',');
                        foreach (string header in headers)
                        {
                            dt.Columns.Add(header);
                        }
                        while (!sr.EndOfStream)
                        {
                            string[] rows = sr.ReadLine().Split(',');
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                dr[i] = rows[i];
                            }
                            dt.Rows.Add(dr);
                        }
                    }

                    var Jsondata = DataTableToJSONWithJavaScriptSerializer(dt);
                    return Json(Jsondata);
                }
                else
                {
                    return Json("No File Saved.");
                }
            }
            catch (Exception)
            {
                return Json("Error While Saving.");
            }
        }
        //
        [HttpPost]
        public ActionResult Demo()
        {
            try
            {
                if (Request.Files["file"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["file"].FileName).ToLower();
                    // string query = null;
                    string connString = "";
                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads"), Request.Files["file"].FileName);
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Uploads"));
                    }
                    if (validFileTypes.Contains(extension))
                    {
                        if (System.IO.File.Exists(path1))
                        { System.IO.File.Delete(path1); }
                        Request.Files["file"].SaveAs(path1);
                        if (extension == ".csv")
                        {
                            DataTable dt = ConvertCSVtoDataTable(path1);
                            ViewBag.Data = dt;
                        }
                        //Connection String to Excel Workbook  
                        else if (extension.Trim() == ".xls")
                        {
                            connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            DataTable dt = ConvertXSLXtoDataTable(path1, connString);
                            ViewBag.Data = dt;
                        }
                        else if (extension.Trim() == ".xlsx")
                        {
                            connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            DataTable dt = ConvertXSLXtoDataTable(path1, connString);
                            ViewBag.Data = dt;
                        }

                    }
                    else
                    {
                        ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";

                    }
                }
                return View();
            }
            catch (Exception)
            {
                return Json("Error While Saving.");
            }
        }
        //
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }

            }


            return dt;
        }
        //
        public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            try
            {

                oledbConn.Open();
                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn);
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                oleda.SelectCommand = cmd;
                DataSet ds = new DataSet();
                oleda.Fill(ds);

                dt = ds.Tables[0];

            }
            catch
            {
            }
            finally
            {

                oledbConn.Close();
            }

            return dt;

        }
        // Data Convertion to Json
        public static string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            //if (strSearch == null || strSearch == String.Empty || strSearch.Trim().Length == 0)
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }

            return jsSerializer.Serialize(parentRow);
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

                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
    }
}
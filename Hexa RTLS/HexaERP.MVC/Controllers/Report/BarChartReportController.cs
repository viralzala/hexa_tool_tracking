using HexaERP.MVC.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.Report
{
    public class BarChartReportController : Controller
    {
        string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["ERPdbEntities"].ConnectionString; // Here is your connection string
        private ERPdbEntities db = new ERPdbEntities();
        // GET: BarChartReport
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


        //public ActionResult GetCount()
        //{
        //    //string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["SchoolContext"].ConnectionString; // Here is your connection string
        //    SqlConnection cnn = new SqlConnection(cnnString);
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.Connection = cnn;
        //    cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //    cmd.CommandText = "spGetCountRecord"; // Here is the name of your stored procedure
        //    cnn.Open();
        //    SqlDataReader o = cmd.ExecuteReader();
        //    List<CountEntity> p = new List<CountEntity>();
        //    cnn.Close();
        //    //var _fromDate = new SqlParameter("@fromDate", DateTime.Now);
        //    //var _toDate = new SqlParameter("@fromDate", DateTime.Now);
        //    //var result = db.Database.SqlQuery<CountEntity>("EXEC spGetCountRecord");
        //    return Json(p, JsonRequestBehavior.AllowGet);
        //}
        public DateTime ConvertToDateTime(string strDateTime)
        {
            DateTime dtFinaldate; string sDateTime;
            try { dtFinaldate = Convert.ToDateTime(strDateTime); }
            catch (Exception)
            {
                string[] sDate = strDateTime.Split('/');
                sDateTime = sDate[1] + '/' + sDate[0] + '/' + sDate[2];
                dtFinaldate = Convert.ToDateTime(sDateTime);
            }
            return dtFinaldate;
        }

        [HttpPost]
        public ActionResult GetCount(SummaryReport obj)
        {

            // Initialization.    
            JsonResult result = new JsonResult();
            // var CompHeader = string.Empty; var CompData = string.Empty;
            try
            {
                DateTime fDate = ConvertToDateTime(obj.fromDate);
                DateTime tDate = ConvertToDateTime(obj.toDate);

                if (obj == null || obj == null)
                {
                    return Json(new { Flag = false, Message = "Between Date Require" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Idata = db.spGetCountRecord(fDate, tDate).ToList();

                    if (Idata != null)
                    {

                        var mEmpc = db.EmployeeAttCounts(fDate, tDate).ToList();


                        var ComWise = mEmpc.GroupBy(n => new { n.Zone, n.Tdate }, (key, group)
                       => new
                       {
                           y = group.Count(),
                           Tdate = ((DateTime)(key.Tdate)).ToShortDateString(),
                           name = key.Zone
                       }).OrderBy(n => n.Tdate);




                        var WorkCatWise = Idata.GroupBy(n => new { n.WorkCategory, n.Tdate }, (key, group)
                           => new
                           {
                               name = key.WorkCategory,
                               Tdate = ((DateTime)(key.Tdate)).ToShortDateString(),
                               y = group.Count()
                           }).OrderBy(n => n.Tdate);

                        var AgenWise = Idata.GroupBy(n => new { n.Agency, n.Tdate }, (key, group)
                               => new
                               {
                                   name = key.Agency,
                                   Tdate = ((DateTime)(key.Tdate)).ToShortDateString(),
                                   y = group.Count()
                               }).OrderBy(n => n.Tdate);

                        var DesCatWise = Idata.GroupBy(n => new { n.Designation, n.Tdate }, (key, group)
                     => new
                     {
                         name = key.Designation,
                         Tdate = ((DateTime)(key.Tdate)).ToShortDateString(),
                         y = group.Count()
                     }).OrderBy(n => n.Tdate);




                        DataTable CompWDT = ToDataTable(ComWise.ToList());
                        var newDt = GetInversedDataTable(CompWDT, "Tdate", "name", "y", "0", true);
                        var CompHeader = HighChartHeader(newDt);
                        var CompData = HighChartData(newDt);


                        DataTable WorkCatDT = ToDataTable(WorkCatWise.ToList());
                        var newWorkCat = GetInversedDataTable(WorkCatDT, "Tdate", "name", "y", "0", true);
                        var WorkCatHeader = HighChartHeader(newWorkCat);
                        var WorkCatData = HighChartData(newWorkCat);


                        DataTable AgenDT = ToDataTable(AgenWise.ToList());
                        var newAgen = GetInversedDataTable(AgenDT, "Tdate", "name", "y", "0", true);
                        var AgenHeader = HighChartHeader(newAgen);
                        var AgenData = HighChartData(newAgen);

                        DataTable DesCatDT = ToDataTable(DesCatWise.ToList());
                        var newDesCat = GetInversedDataTable(DesCatDT, "Tdate", "name", "y", "0", true);
                        var DesCatHeader = HighChartHeader(newDesCat);
                        var DesCatData = HighChartData(newDesCat);

                        //var CompartWise = JsonConvert.SerializeObject(_data.ToArray(), new KeyValuePairConverter());
                        result = this.Json(new { CompHeader, CompData = CompData.ToArray(), WorkCatHeader, WorkCatData = WorkCatData.ToArray(), AgenHeader, AgenData = AgenData.ToArray(), DesCatHeader, DesCatData = DesCatData.ToArray() }, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {
                        result = this.Json(new { Flag = false, Message = "Data Not Found", data = Idata }, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch (Exception ex)
            {
                result = this.Json(new { Flag = false, Message = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        private List<HisghChartEntity> HighChartData(DataTable tbl)
        {
            List<HisghChartEntity> _data = new List<HisghChartEntity>();
            HisghChartEntity _names = new HisghChartEntity();
            _data.Clear();

            List<int> primes = new List<int>();

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                primes.Clear();

                for (int j = 0; j < tbl.Columns.Count; j++)
                {
                    if (tbl.Rows[i].ItemArray[j].ToString().All(Char.IsDigit))
                    {
                        primes.Add(Convert.ToInt32(tbl.Rows[i].ItemArray[j]));
                    }
                    else
                    {
                        _names.name = tbl.Rows[i].ItemArray[j].ToString();
                    }
                }

                _data.Add(new HisghChartEntity()
                {
                    name = _names.name.ToString(),
                    data = primes.ToArray()
                });
            }
            return _data;
        }


        private List<string> HighChartHeader(DataTable tbl)
        {
            List<string> _header = new List<string>();
            _header.Clear();
            foreach (DataColumn dc in tbl.Columns)
            {
                string Header = dc.ColumnName;
                if (Header != "name")
                {
                    _header.Add(Header);
                }
            }
            return _header;
        }

        public ArrayList ConvertDT(ref DataTable dt)
        {
            ArrayList converted = new ArrayList(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                converted.Add(row);
            }
            return converted;
        }
        //
        private DataTable Pivot(DataTable tbl)
        {
            var tblPivot = new DataTable();
            tblPivot.Columns.Add(tbl.Columns[0].ColumnName);
            for (int i = 1; i < tbl.Rows.Count; i++)
            {
                tblPivot.Columns.Add(Convert.ToString(i));
            }
            for (int col = 0; col < tbl.Columns.Count; col++)
            {
                var r = tblPivot.NewRow();
                r[0] = tbl.Columns[col].ToString();
                for (int j = 1; j < tbl.Rows.Count; j++)
                    r[j] = tbl.Rows[j][col];

                tblPivot.Rows.Add(r);
            }
            return tblPivot;
        }
        //
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        //
        public static DataTable GetInversedDataTable(DataTable table, string columnX, string columnY, string columnZ, string nullValue, bool sumValues)
        {
            //Create a DataTable to Return
            DataTable returnTable = new DataTable();

            if (columnX == "")
                columnX = table.Columns[0].ColumnName;

            //Add a Column at the beginning of the table
            returnTable.Columns.Add(columnY);


            //Read all DISTINCT values from columnX Column in the provided DataTale
            List<string> columnXValues = new List<string>();

            foreach (DataRow dr in table.Rows)
            {

                string columnXTemp = dr[columnX].ToString();
                if (!columnXValues.Contains(columnXTemp))
                {
                    //Read each row value, if it's different from others provided, add to the list of values and creates a new Column with its value.
                    columnXValues.Add(columnXTemp);
                    returnTable.Columns.Add(columnXTemp);
                }
            }

            //Verify if Y and Z Axis columns re provided
            if (columnY != "" && columnZ != "")
            {
                //Read DISTINCT Values for Y Axis Column
                List<string> columnYValues = new List<string>();

                foreach (DataRow dr in table.Rows)
                {
                    if (!columnYValues.Contains(dr[columnY].ToString()))
                        columnYValues.Add(dr[columnY].ToString());
                }

                //Loop all Column Y Distinct Value
                foreach (string columnYValue in columnYValues)
                {
                    //Creates a new Row
                    DataRow drReturn = returnTable.NewRow();
                    drReturn[0] = columnYValue;
                    //foreach column Y value, The rows are selected distincted
                    DataRow[] rows = table.Select(columnY + "='" + columnYValue + "'");

                    //Read each row to fill the DataTable
                    foreach (DataRow dr in rows)
                    {
                        string rowColumnTitle = dr[columnX].ToString();

                        //Read each column to fill the DataTable
                        foreach (DataColumn dc in returnTable.Columns)
                        {
                            if (dc.ColumnName == rowColumnTitle)
                            {
                                //If Sum of Values is True it try to perform a Sum
                                //If sum is not possible due to value types, the value displayed is the last one read
                                if (sumValues)
                                {
                                    try
                                    {
                                        drReturn[rowColumnTitle] = Convert.ToDecimal(drReturn[rowColumnTitle]) + Convert.ToDecimal(dr[columnZ]);
                                    }
                                    catch
                                    {
                                        drReturn[rowColumnTitle] = dr[columnZ];
                                    }
                                }
                                else
                                {
                                    drReturn[rowColumnTitle] = dr[columnZ];
                                }

                            }
                        }
                    }

                    returnTable.Rows.Add(drReturn);
                }

            }
            else
            {
                throw new Exception("The columns to perform inversion are not provided");
            }

            //if a nullValue is provided, fill the datable with it
            if (nullValue != "")
            {
                foreach (DataRow dr in returnTable.Rows)
                {
                    foreach (DataColumn dc in returnTable.Columns)
                    {
                        if (dr[dc.ColumnName].ToString() == "")
                            dr[dc.ColumnName] = nullValue;
                    }
                }
            }

            return returnTable;
        }
        //
        public static DataTable GetInversedDataTable(DataTable table, string columnX, params string[] columnsToIgnore)
        {
            //Create a DataTable to Return
            DataTable returnTable = new DataTable();

            if (columnX == "")
                columnX = table.Columns[0].ColumnName;

            //Add a Column at the beginning of the table
            returnTable.Columns.Add(columnX);

            //Read all DISTINCT values from columnX Column in the provided DataTale
            List<string> columnXValues = new List<string>();


            //Creates list of columns to ignore
            List<string> listColumnsToIgnore = new List<string>();
            if (columnsToIgnore.Length > 0)
                listColumnsToIgnore.AddRange(columnsToIgnore);

            if (!listColumnsToIgnore.Contains(columnX))
                listColumnsToIgnore.Add(columnX);

            foreach (DataRow dr in table.Rows)
            {
                string columnXTemp = dr[columnX].ToString();
                //Verify if the value was already listed
                if (!columnXValues.Contains(columnXTemp))
                {
                    //if the value id different from others provided, add to the list of values and creates a new Column with its value.
                    columnXValues.Add(columnXTemp);
                    returnTable.Columns.Add(columnXTemp);
                }
                else
                {
                    //Throw exception for a repeated value
                    throw new Exception("The inversion used must have unique values for column " + columnX);
                }
            }

            //Add a line for each column of the DataTable
            foreach (DataColumn dc in table.Columns)
            {
                if (!columnXValues.Contains(dc.ColumnName) && !listColumnsToIgnore.Contains(dc.ColumnName))
                {
                    DataRow dr = returnTable.NewRow();
                    dr[0] = dc.ColumnName;
                    returnTable.Rows.Add(dr);
                }
            }

            //Complete the datatable with the values
            for (int i = 0; i < returnTable.Rows.Count; i++)
            {
                for (int j = 1; j < returnTable.Columns.Count; j++)
                {
                    returnTable.Rows[i][j] = table.Rows[j - 1][returnTable.Rows[i][0].ToString()].ToString();
                }
            }

            return returnTable;
        }
    }

    public partial class HisghChartEntity
    {
        public string name { get; set; }
        public int[] data { get; set; }
    }

    public partial class HisghChartHeader
    {
        public string[] header { get; set; }
    }

    public class CountEntity
    {
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public string Agency { get; set; }
        public string WorkCategory { get; set; }
        public string Designation { get; set; }
        public string Site { get; set; }
        public string Zone { get; set; }
        public string FloorName { get; set; }
        public string RFID { get; set; }
        public Nullable<System.DateTime> Tdate { get; set; }
    }

}


namespace HexaERP.MVC.Controllers.RFID
{
    //public class AssetIssueController : Controller
    //{
    //    //******************
    //    //Author: Mudassar I
    //    //Date: 02/03/2017
    //    //FloorMasterController
    //    //******************
    //    // Create an instance of the ImpinjReader class.
    //    static ImpinjReader reader = new ImpinjReader();

    //    // Create a Dictionary to store the tags we've read.
    //    static Dictionary<string, Tag> tagsRead = new Dictionary<string, Tag>();
    //    static List<entitys> tags = new List<entitys>();
    //    static List<EntityOfMsg> MsgList = new List<EntityOfMsg>();
    //    private ERPdbEntities db = new ERPdbEntities();
    //    // GET: 
    //    public ActionResult Index()
    //    {            
    //        return View();
    //    }
    //    [HttpGet]
    //    public void DeleteAssetMonitor() {
    //        try
    //        {
    //            tagsRead.Clear();
    //            tags.Clear(); 
    //            db.Database.ExecuteSqlCommand("DELETE FROM tAssetMonitor");
    //            db.SaveChanges();               
    //        }
    //        catch (Exception) { }

    //    }
    //    [HttpGet]
    //    public string ReaderInit(string Reader)
    //    {
    //        string msg = "";

    //        if (Reader == "")
    //        {
    //            return msg = "Please Select Reader IP Address To Connect";
    //        }
    //        try
    //        {

    //            // Connect to the reader.
    //            // Change the ReaderHostname constant in SolutionConstants.cs 
    //            // to the IP address or hostname of your reader.
    //            reader.Connect(Reader);

    //            Status status = reader.QueryStatus();

    //            try
    //            {
    //                // Don't call the Stop method if the
    //                // reader is already stopped.
    //                if (reader.QueryStatus().IsSingulating)
    //                {
    //                    reader.Stop();
    //                }
    //            }
    //            catch (OctaneSdkException ex)
    //            {
    //                // An Octane SDK exception occurred. Handle it here.                  
    //                return msg = "('An Octane SDK exception has occurred : {0}', " + ex.Message + ")";
    //            }
    //            catch (Exception ex)
    //            {
    //                // A general exception occurred. Handle it here.                    
    //                return msg = "('An Octane SDK exception has occurred : {0}', " + ex.Message + ")";
    //            }
    //            // Get the default settings
    //            // We'll use these as a starting point
    //            // and then modify the settings we're 
    //            // interested in.
    //            Settings settings = reader.QueryDefaultSettings();

    //            // Tell the reader to include the TID
    //            // in all tag reports. We will use FastID
    //            // to do this. FastID is supported
    //            // by Impinj Monza 4 and later tags.
    //            settings.Report.IncludeFastId = true;
    //            settings.Report.IncludeAntennaPortNumber = true;
    //            // Apply the newly modified settings.
    //            reader.ApplySettings(settings);
    //            // Assign the TagsReported event handler.
    //            // This specifies which method to call
    //            // when tags reports are available.
    //            reader.TagsReported += OnTagsReported;

    //            // Start reading.
    //            reader.Start();

    //            msg = "Reader started...";
    //            //// Wait for the user to press enter.
    //            //Console.WriteLine("Press enter to exit.");
    //            //Console.ReadLine();

    //            //// Stop reading.
    //            //reader.Stop();

    //            //// Disconnect from the reader.
    //            //reader.Disconnect();
    //        }
    //        catch (OctaneSdkException e)
    //        {
    //            // Handle Octane SDK errors.                
    //            msg = e.Message.ToString();
    //        }
    //        catch (Exception e)
    //        {
    //            // Handle other .NET errors.               
    //            msg = e.Message.ToString();
    //        }
    //        return msg;
    //    }
    //    //
    //    public void OnTagsReported(ImpinjReader sender, TagReport report)
    //    {
    //        tAssetMonitor obj = new tAssetMonitor();
    //        //entitys on = new entitys();
    //        // This event handler is called asynchronously 
    //        // when tag reports are available.
    //        // Loop through each tag in the report 
    //        // and print the data.
    //        foreach (Tag tag in report)
    //        {
    //            // If this tag hasn't been read before, print out the EPC and TID
    //            if (!tagsRead.ContainsKey(tag.Epc.ToString()))
    //            {
    //                // Add this tag to the list of tags we've read.
    //                tagsRead.Add(tag.Epc.ToString(), tag);
    //                try
    //                {
    //                    obj.RFID = tag.Epc.ToString(); obj.mAttPortId = tag.AntennaPortNumber;
    //                    db.tAssetMonitors.Add(obj);
    //                    db.SaveChanges();
    //                }
    //                catch (Exception) {}
    //            }
    //        }

    //    }
    //    //
    //    [HttpGet]
    //    public JsonResult GetIds()
    //    {           
    //        var UserName = Session["AppUserName"];
    //        //Get Organization Id From Session Variable
    //        int orgId = Convert.ToInt32(Session["OrgInfoId"]);
    //        //Get Selected Data Accourding to Org Id
    //        var ObjData = (from am in db.tAssetMonitors
    //                       join atg in db.tAssetTags on am.RFID equals atg.RFID //into CUT_FLR
    //                       join im in db.mIteamMasters on atg.mIteamMasterId equals im.mIteamMasterId into imi
    //                       join um in db.mUnitMasters on atg.mUnitMasterId equals um.mUnitMasterId into umi
    //                       join gm in db.mGroupMasters on atg.mGroupMasterId equals gm.mGroupMasterId into gmi
    //                       join ity in db.mIteamTypeMasters on atg.mIteamTypeMasterId equals ity.mIteamTypeMasterId into ityi
    //                       join rm in db.mRoomMasters on atg.mRoomMasterId equals rm.mRoomMasterId into rmi
    //                       where (atg.OrgInfoId == orgId && atg.IsAction==true)
    //                       from im in imi.DefaultIfEmpty()
    //                       from um in umi.DefaultIfEmpty()
    //                       from gm in gmi.DefaultIfEmpty()
    //                       from ity in ityi.DefaultIfEmpty()
    //                       from rm in rmi.DefaultIfEmpty()
    //                       select new {
    //                           am.RFID,
    //                           am.Id,
    //                           atg.tAssetTagId,
    //                           AssetName =atg.IteamName,
    //                           atg.IteamCode,
    //                           atg.IteamDescription,
    //                           atg.Stock,
    //                           atg.UID,
    //                           atg.bStock,
    //                           im.IteamName,
    //                           um.UnitName,
    //                           gm.GroupName,
    //                           ity.IteamType,
    //                           rm.RoomName,
    //                           rm.RoomNo,
    //                           atg.mRoomMasterId }).ToList();          
    //            return Json(ObjData, JsonRequestBehavior.AllowGet);                     
    //    }
    //    //
    //    [HttpGet]
    //    public JsonResult getGetFloorsData()
    //    {
    //        //Get Organization Id From Session Variable
    //        int orgId = Convert.ToInt32(Session["OrgInfoId"]);
    //        //Get Selected Data Accourding to Org Id
    //        var ObjData = (from Dis in db.mFloorMasters
    //                       where (Dis.OrgInfoId == orgId && Dis.IsAction == true)
    //                       select new { Dis.mFloorMasterId, Dis.FloorName }).ToList();
    //        //Convert List Data to The Json Array          
    //        return Json(ObjData, JsonRequestBehavior.AllowGet);
    //    }
    //    //
    //    [HttpGet]
    //    public JsonResult getGetRoomsData(int FloorId)
    //    {
    //        //Get Organization Id From Session Variable
    //        int orgId = Convert.ToInt32(Session["OrgInfoId"]);
    //        //Get Selected Data Accourding to Org Id
    //        var ObjData = (from Dis in db.mRoomMasters
    //                       where (Dis.OrgInfoId == orgId && Dis.mFloorMasterId == FloorId && Dis.IsAction == true)
    //                       select new { Dis.mRoomMasterId, Dis.RoomName }).ToList();
    //        //Convert List Data to The Json Array          
    //        return Json(ObjData, JsonRequestBehavior.AllowGet);
    //    }
    //    //
    //    [HttpGet]
    //    public string ReaderClear()
    //    {
    //        string msg = "";
    //        try
    //        {
    //            tags.Clear();
    //            tagsRead.Clear();
    //            msg = "Data Cleared..";
    //        }
    //        catch (Exception ex)
    //        {
    //            msg = ex.Message.ToString();
    //        }
    //        return msg;
    //    }
    //    //
    //    [HttpGet]
    //    public string StopReaders()
    //    {
    //        string msg = "";
    //        try
    //        {
    //            // Don't call the Stop method if the
    //            // reader is already stopped.
    //            if (reader.QueryStatus().IsSingulating)
    //            {
    //                reader.Stop();
    //                msg = "Reader stopped..";
    //            }
    //        }
    //        catch (OctaneSdkException ex)
    //        {
    //            // An Octane SDK exception occurred. Handle it here.
    //            msg = ex.Message.ToString();
    //        }
    //        catch (Exception ex)
    //        {
    //            // A general exception occurred. Handle it here.
    //            msg = ex.Message.ToString();
    //        }
    //        return msg;
    //    }
    //    public class entitys
    //    {
    //        public string RFID;
    //        public int PORTID;
    //    }
    //    //
    //    [HttpGet]
    //    public JsonResult getGetReadersData()
    //    {
    //        //Get Organization Id From Session Variable
    //        int orgId = Convert.ToInt32(Session["OrgInfoId"]);
    //        //Get Selected Data Accourding to Org Id
    //        var ObjData = (from Dis in db.mReaders
    //                       where (Dis.OrgInfoId == orgId && Dis.IsAction == true && Dis.ReaderIP != null)
    //                       select new { Dis.ReaderIP }).ToList().Distinct();
    //        //Convert List Data to The Json Array          
    //        return Json(ObjData, JsonRequestBehavior.AllowGet);
    //    }

    //    public class EntityOfMsg{
    //        public string msg;
    //        public string IteamName;
    //        public int tAssetTagIds;
    //    }
    //    //
    //    [HttpGet]
    //    public JsonResult StockIssue(string IssuedTo, string IteamList)
    //    {
    //        // List<EntityOfMsg> MsgList = new List<EntityOfMsg>();
    //        MsgList.Clear();
    //        string msg = "";
    //        try
    //        {
    //            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);   
    //            List<tAssetStockIssue> obj = JsonConvert.DeserializeObject<List<tAssetStockIssue>>(IteamList);
    //            using (var context = new ERPdbEntities())
    //            {
    //                EntityOfMsg msgObj = new EntityOfMsg();
    //                foreach (var Iteam in obj) {

    //                    if (Iteam.tAssetTagId == null)
    //                    {
    //                        msgObj.msg= "Asset Id Is Null";
    //                        msgObj.IteamName = Iteam.tAssetTagId.ToString();
    //                        msgObj.tAssetTagIds = Convert.ToInt32(Iteam.tAssetTagId);
    //                        MsgList.Add(msgObj);

    //                    }
    //                    else {

    //                        var ObData = context.tAssetTags
    //                                       .Where(b => b.tAssetTagId == Iteam.tAssetTagId && b.IsAction==true)
    //                                        .ToList();
    //                        //var FoundId = ObData.TC_CustomerId;                    
    //                        if (ObData.Any())
    //                        {

    //                            var FoundId = ObData.Single();

    //                            if (FoundId.bStock == null || FoundId.bStock == 0)
    //                            {                                   
    //                                MsgList.Add(new EntityOfMsg {
    //                                msg = "Stock Not Availabe or Else you are alredy Isuued this Asset",
    //                                IteamName = FoundId.IteamName.ToString(),
    //                                tAssetTagIds = Convert.ToInt32(FoundId.tAssetTagId)
    //                            });                                   
    //                            }
    //                            else {
    //                                string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("x");
    //                                tAssetStockIssue tasObj = new tAssetStockIssue();

    //                                tasObj.tAssetTagId = FoundId.tAssetTagId;
    //                                tasObj.UID = FoundId.UID;
    //                                tasObj.RFID = FoundId.RFID;
    //                                tasObj.Stock = FoundId.Stock;
    //                                tasObj.IssuedTo = IssuedTo;
    //                                tasObj.InvoiceNo = Uid;
    //                                tasObj.IsAction = true;
    //                                //
    //                                tasObj.OrgInfoId = orgId;
    //                                tasObj.IssuedDate = DateTime.Now;
    //                                tasObj.IssuedBy = UserName.ToString();
    //                                context.tAssetStockIssues.Add(tasObj);
    //                                context.SaveChanges();

    //                                FoundId.Stock = 0;
    //                                FoundId.bStock = FoundId.Stock;
    //                                FoundId.ModifiedBy = UserName.ToString();
    //                                FoundId.ModifiedDate = DateTime.Now;
    //                                context.SaveChanges();


    //                                MsgList.Add(new EntityOfMsg {
    //                                msg = "Asset Tranfered Sucessfully",
    //                                IteamName = FoundId.IteamName.ToString(),
    //                                tAssetTagIds = Convert.ToInt32(FoundId.tAssetTagId)
    //                            });

    //                            }                               
    //                        }
    //                        else
    //                        {                              
    //                            MsgList.Add(new EntityOfMsg {
    //                            msg = "This Asset Is Not Available Or Not Register",
    //                            IteamName = Iteam.tAssetTagId.ToString(),
    //                            tAssetTagIds = Convert.ToInt32(Iteam.tAssetTagId)
    //                        });

    //                        }
    //                    }
    //                }
    //            }               
    //        }
    //        catch (Exception ex)
    //        {
    //            msg = ex.Message.ToString();
    //        }
    //        return Json(MsgList.ToArray(), JsonRequestBehavior.AllowGet);            
    //    }

    //    [HttpGet]
    //    public JsonResult StockTranfer(int mRoomMasterId, string IteamList)
    //    {
    //        // List<EntityOfMsg> MsgList = new List<EntityOfMsg>();

    //        MsgList.Clear();
    //        string msg = "";
    //        try
    //        {
    //            var UserName = Session["AppUserName"]; int orgId = Convert.ToInt32(Session["OrgInfoId"]);
    //            List<tAssetStockTranfer> obj = JsonConvert.DeserializeObject<List<tAssetStockTranfer>>(IteamList);
    //            using (var context = new ERPdbEntities())
    //            {
    //                EntityOfMsg msgObj = new EntityOfMsg();
    //                foreach (var Iteam in obj)
    //                {

    //                    if (Iteam.tAssetTagId == null)
    //                    {
    //                        msgObj.msg = "Asset Id Is Null";
    //                        msgObj.IteamName = Iteam.tAssetTagId.ToString();
    //                        msgObj.tAssetTagIds = Convert.ToInt32(Iteam.tAssetTagId);
    //                        MsgList.Add(msgObj);

    //                    }
    //                    else
    //                    {

    //                        var ObData = context.tAssetTags
    //                                       .Where(b => b.tAssetTagId == Iteam.tAssetTagId && b.IsAction == true)
    //                                        .ToList();
    //                        //var FoundId = ObData.TC_CustomerId;                    
    //                        if (ObData.Any())
    //                        {

    //                            var FoundId = ObData.Single();
    //                            if ((FoundId.bStock == null || FoundId.bStock == 0))
    //                            {
    //                                MsgList.Add(new EntityOfMsg
    //                                {
    //                                    msg = "Stock Not Availabe or Else you are alredy Isuued this Asset",
    //                                    IteamName = FoundId.IteamName.ToString(),
    //                                    tAssetTagIds = Convert.ToInt32(FoundId.tAssetTagId)
    //                                });
    //                            }
    //                            else if (FoundId.mRoomMasterId == mRoomMasterId) {
    //                                MsgList.Add(new EntityOfMsg
    //                                {
    //                                    msg = "Stock is alredy belongs to same warehouse",
    //                                    IteamName = FoundId.IteamName.ToString(),
    //                                    tAssetTagIds = Convert.ToInt32(FoundId.tAssetTagId)
    //                                });
    //                            }
    //                            else
    //                            {
    //                                string Uid = Guid.NewGuid().ToString().GetHashCode().ToString("x");
    //                                tAssetStockTranfer tasObj = new tAssetStockTranfer();

    //                                tasObj.tAssetTagId = FoundId.tAssetTagId;
    //                                tasObj.UID = FoundId.UID;
    //                                tasObj.RFID = FoundId.RFID;
    //                                tasObj.Stock = FoundId.bStock;
    //                                tasObj.PicupBy = "";
    //                                tasObj.mRoomMasterId = mRoomMasterId;

    //                                //
    //                                tasObj.OrgInfoId = orgId;
    //                                tasObj.TranferDate = DateTime.Now;
    //                                tasObj.TranferBy = UserName.ToString();
    //                                context.tAssetStockTranfers.Add(tasObj);
    //                                context.SaveChanges();


    //                                FoundId.mRoomMasterId = mRoomMasterId;
    //                                FoundId.Stock = FoundId.Stock;
    //                                FoundId.bStock = FoundId.bStock;
    //                                FoundId.ModifiedBy = UserName.ToString();
    //                                FoundId.ModifiedDate = DateTime.Now;
    //                                context.SaveChanges();


    //                                MsgList.Add(new EntityOfMsg
    //                                {
    //                                    msg = "Asset Tranfered Sucessfully",
    //                                    IteamName = FoundId.IteamName.ToString(),
    //                                    tAssetTagIds = Convert.ToInt32(FoundId.tAssetTagId)
    //                                });

    //                            }
    //                        }
    //                        else
    //                        {
    //                            MsgList.Add(new EntityOfMsg
    //                            {
    //                                msg = "This Asset Is Not Available Or Not Register",
    //                                IteamName = Iteam.tAssetTagId.ToString(),
    //                                tAssetTagIds = Convert.ToInt32(Iteam.tAssetTagId)
    //                            });

    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            msg = ex.Message.ToString();
    //        }
    //        return Json(MsgList.ToArray(), JsonRequestBehavior.AllowGet);
    //    }
    //}
}
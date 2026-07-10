using HexaERP.MVC.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HexaERP.MVC.Controllers.Tools
{
    public class ToolTrackingController : ApiController
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: api/ToolTracking
        public IQueryable<tAssetTag> GettAssetTags()
        {
            return db.tAssetTags;
        }

        // GET: api/ToolTracking/5
        [ResponseType(typeof(tAssetTag))]
        public IHttpActionResult GettAssetTag(int id)
        {
            tAssetTag tAssetTag = db.tAssetTags.Find(id);
            if (tAssetTag == null)
            {
                return NotFound();
            }

            return Ok(tAssetTag);
        }

        // PUT: api/ToolTracking/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PuttAssetTag(int id, tAssetTag tAssetTag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tAssetTag.tAssetTagId)
            {
                return BadRequest();
            }

            db.Entry(tAssetTag).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tAssetTagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ToolTracking
        [ResponseType(typeof(tAssetTag))]
        public IHttpActionResult PosttAssetTag(tAssetTag tAssetTag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.tAssetTags.Add(tAssetTag);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = tAssetTag.tAssetTagId }, tAssetTag);
        }

        // DELETE: api/ToolTracking/5
        [ResponseType(typeof(tAssetTag))]
        public IHttpActionResult DeletetAssetTag(int id)
        {
            tAssetTag tAssetTag = db.tAssetTags.Find(id);
            if (tAssetTag == null)
            {
                return NotFound();
            }

            db.tAssetTags.Remove(tAssetTag);
            db.SaveChanges();

            return Ok(tAssetTag);
        }

        public class ToolIssueModelView
        {
            public string rfid { get; set; }
            public string PartNo { get; set; }
            public DateTime CalibrationDueDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public int EmployeeID { get; set; }
        }
        [HttpPost]
        [Route("api/ToolTracking/ToolIssue")]
        public async Task<IHttpActionResult> ToolIssue(ToolIssueModelView toolIssueModelView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(toolIssueModelView.rfid))
            {
                return BadRequest();
            }


            var findDetails = await db.tAssetTags
                .Where(x => x.RFID == toolIssueModelView.rfid || x.IteamCode == toolIssueModelView.PartNo)
                .FirstOrDefaultAsync();

            if (findDetails == null)
            {
                return NotFound();
            }

            findDetails.tEmployeeTagId = toolIssueModelView.EmployeeID;
            findDetails.IssueDate = DateTime.Now;
            findDetails.ReturnDate = toolIssueModelView.ExpiryDate;
            db.Entry(findDetails).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Ok(findDetails);

        }
        [HttpPost]
        [Route("api/ToolTracking/ToolReturn")]
        public async Task<IHttpActionResult> ToolReturn(ToolIssueModelView toolIssueModelView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(toolIssueModelView.rfid))
            {
                return BadRequest();
            }


            var findDetails = await db.tAssetTags
                .Where(x => x.RFID == toolIssueModelView.rfid || x.IteamCode == toolIssueModelView.PartNo)
                .FirstOrDefaultAsync();

            if (findDetails == null)
            {
                return NotFound();
            }

            findDetails.tEmployeeTagId = null;
            findDetails.IssueDate = null;
            findDetails.ReturnDate = DateTime.Now;
            db.Entry(findDetails).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Ok(findDetails);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool tAssetTagExists(int id)
        {
            return db.tAssetTags.Count(e => e.tAssetTagId == id) > 0;
        }
    }
}
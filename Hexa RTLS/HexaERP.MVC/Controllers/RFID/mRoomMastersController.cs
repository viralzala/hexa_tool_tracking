using HexaERP.MVC.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace HexaERP.MVC.Controllers.RFID
{
    public class mRoomMastersController : Controller
    {
        private ERPdbEntities db = new ERPdbEntities();

        // GET: mRoomMasters
        public ActionResult Index()
        {
            return View(db.mRoomMasters.ToList());
        }

        // GET: mRoomMasters/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mRoomMaster mRoomMaster = db.mRoomMasters.Find(id);
            if (mRoomMaster == null)
            {
                return HttpNotFound();
            }
            return View(mRoomMaster);
        }

        // GET: mRoomMasters/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: mRoomMasters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "mRoomMasterId,mFloorMasterId,RoomName,RoomNo,IsAction,OrgInfoId,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy")] mRoomMaster mRoomMaster)
        {
            if (ModelState.IsValid)
            {
                db.mRoomMasters.Add(mRoomMaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mRoomMaster);
        }

        // GET: mRoomMasters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mRoomMaster mRoomMaster = db.mRoomMasters.Find(id);
            if (mRoomMaster == null)
            {
                return HttpNotFound();
            }
            return View(mRoomMaster);
        }

        // POST: mRoomMasters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "mRoomMasterId,mFloorMasterId,RoomName,RoomNo,IsAction,OrgInfoId,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy")] mRoomMaster mRoomMaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mRoomMaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mRoomMaster);
        }

        // GET: mRoomMasters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mRoomMaster mRoomMaster = db.mRoomMasters.Find(id);
            if (mRoomMaster == null)
            {
                return HttpNotFound();
            }
            return View(mRoomMaster);
        }

        // POST: mRoomMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            mRoomMaster mRoomMaster = db.mRoomMasters.Find(id);

            var _mReaderSettup = $"DELETE FROM mReaderSettup Where mZoneId = {mRoomMaster.mZoneId}";
            db.Database.ExecuteSqlCommand(_mReaderSettup);


            db.mRoomMasters.Remove(mRoomMaster);
            db.SaveChanges();



            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

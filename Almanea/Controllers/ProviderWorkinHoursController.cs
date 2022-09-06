using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Almanea;
using Almanea.BusinessLogic;
using Almanea.Models;
using Almanea.Data;

namespace Almanea.Controllers
{
    public class ProviderWorkinHoursController : BaseController
    {
        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        // GET: tblProviderWorkinHours
        [SiteAuthorize("Admin", "SuperAdmin", "Provider")]
        public async Task<ActionResult> Index()
        {
            var providerid = (int)Session[cls_Defaults.Session_UserGroupId];
            var tblProviderWorkinHours = db.tblProviderWorkinHours.Where(z => z.ServiceProviderId == providerid).Include(t => t.tblUserGroupCompany);
            var data = tblProviderWorkinHours.ToListAsync().Result;
            if (data.Count==0)
            {
                ViewBag.adddata = 0;
            }
            else
            {
                ViewBag.adddata = 1;
            } 
            
            return View();
        }
        public async Task<JsonResult> GetWorkingHours()
        {
            var objResult = new vm_Result();
            var providerid = (int)Session[cls_Defaults.Session_UserGroupId];
            var tblProviderWorkinHours = db.tblProviderWorkinHours.Where(z => z.ServiceProviderId == providerid).Include(t => t.tblUserGroupCompany);
            List<ProviderWorkinHourDto> modelList = new List<ProviderWorkinHourDto>();
            ProviderWorkinHourDto model = new ProviderWorkinHourDto();
            foreach (var item in tblProviderWorkinHours)
            {
                model = new ProviderWorkinHourDto();
                model.Id = item.Id;
                model.ServiceProviderId = item.ServiceProviderId;
                model.WorkingHours= item.WorkingHours;
                model.CompanyNameEN = item.tblUserGroupCompany.CompanyNameEN;

                modelList.Add(model);
            }

            
            objResult.Data = modelList;
            objResult.Count = modelList.Count;


            return Json(
                   new
                   {
                       aaData = objResult.Data,
                       sEcho = 0,
                       iTotalRecords = objResult.Count,
                       iTotalDisplayRecords = objResult.Count
                   }
                   , JsonRequestBehavior.AllowGet);
        }
        // GET: tblProviderWorkinHours/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblProviderWorkinHour tblProviderWorkinHour = await db.tblProviderWorkinHours.FindAsync(id);
            if (tblProviderWorkinHour == null)
            {
                return HttpNotFound();
            }
            return View(tblProviderWorkinHour);
        }

        // GET: tblProviderWorkinHours/Create

        [SiteAuthorize("Admin", "SuperAdmin", "Provider")]
        public ActionResult Create()
        {
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN");
            return View();
        }

        // POST: tblProviderWorkinHours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SiteAuthorize("Admin", "SuperAdmin", "Provider")]
        public async Task<ActionResult> Create([Bind(Include = "Id,ServiceProviderId,WorkingHours")] tblProviderWorkinHour tblProviderWorkinHour)
        {
            if (ModelState.IsValid)
            {
                var providerid = (int)Session[cls_Defaults.Session_UserGroupId];
                tblProviderWorkinHour.ServiceProviderId = providerid;
                db.tblProviderWorkinHours.Add(tblProviderWorkinHour);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblProviderWorkinHour.ServiceProviderId);
            return View(tblProviderWorkinHour);
        }

        // GET: tblProviderWorkinHours/Edit/5

        [SiteAuthorize("Admin", "SuperAdmin", "Provider")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var providerid = (int)Session[cls_Defaults.Session_UserGroupId];
            var tblProviderWorkinHour = db.tblProviderWorkinHours.Where(x=>x.ServiceProviderId== providerid).FirstOrDefault();
            if (tblProviderWorkinHour == null)
            {
                return HttpNotFound();
            }
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblProviderWorkinHour.ServiceProviderId);
            return View(tblProviderWorkinHour);
        }

        // POST: tblProviderWorkinHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SiteAuthorize("Admin", "SuperAdmin", "Provider")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ServiceProviderId,WorkingHours")] tblProviderWorkinHour tblProviderWorkinHour)
        {
            if (ModelState.IsValid)
            {
                var providerid = (int)Session[cls_Defaults.Session_UserGroupId];
                tblProviderWorkinHour.ServiceProviderId = providerid;

                db.Entry(tblProviderWorkinHour).State = EntityState.Modified;
                await db.SaveChangesAsync();
                UpdateCapacity((int?)providerid, tblProviderWorkinHour.WorkingHours);
                return RedirectToAction("Index");
            }
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblProviderWorkinHour.ServiceProviderId);
            return View(tblProviderWorkinHour);
        }


        public int UpdateCapacity(int? providerId,int?workinhhour)
        {
            db_User objUser = new db_User();

            db_Settings objSettings = new db_Settings();
            var tblTeamCapacity2 = db.tblTeamCapacities.Where(x => x.ServiceProviderId == providerId).FirstOrDefault();

            var addebysuppl = db.tblUserGroupCompanies.Where(c => c.UserGroupId == providerId).FirstOrDefault();

            var cap = 0;
            var labours = objUser.GetLaboursDopr(Convert.ToInt32(providerId));
            if (labours.Count > 0)
            {
                //spestimat = spestimat + (Convert.ToInt32(spassignedservice.Text) / 60);
                 cap = (labours.Count) * Convert.ToInt32(workinhhour);

                
            }
                


                if (tblTeamCapacity2 != null)
            {
                tblTeamCapacity2.DailyCapacity = cap;
                //tblTeamCapacity2.CurrentCapacity = Convert.ToInt32(tblTeamCapacity2.CurrentCapacity) + dailycap;
                //tblTeamCapacity2.ConsumedCapacity = 0;
                tblTeamCapacity2.ServiceProviderId = providerId;
                tblTeamCapacity2.Updatedate = DateTime.Now;

                db.Entry(tblTeamCapacity2).State = EntityState.Modified;
                db.SaveChanges();
            }

            else
            {
                tblTeamCapacity tblTeamCapacity = new tblTeamCapacity();

                tblTeamCapacity.DailyCapacity = cap;
                tblTeamCapacity.CurrentCapacity = cap;
                tblTeamCapacity.ConsumedCapacity = 0;
                tblTeamCapacity.ServiceProviderId = providerId;
                tblTeamCapacity.Updatedate = DateTime.Now;
                tblTeamCapacity.Supplier = addebysuppl.AddedBy;

                db.tblTeamCapacities.Add(tblTeamCapacity);
                db.SaveChanges();
            }


            return 1;


        }
        // GET: tblProviderWorkinHours/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblProviderWorkinHour tblProviderWorkinHour = await db.tblProviderWorkinHours.FindAsync(id);
            if (tblProviderWorkinHour == null)
            {
                return HttpNotFound();
            }
            return View(tblProviderWorkinHour);
        }

        // POST: tblProviderWorkinHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            tblProviderWorkinHour tblProviderWorkinHour = await db.tblProviderWorkinHours.FindAsync(id);
            db.tblProviderWorkinHours.Remove(tblProviderWorkinHour);
            await db.SaveChangesAsync();
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

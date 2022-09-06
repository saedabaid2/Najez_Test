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
using Almanea.Data;
using Almanea.Models;
using Almanea.BusinessLogic;

namespace Almanea.Controllers
{
    public class tblTeamCapacityCalculationsController : Controller
    {
        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        // GET: tblTeamCapacityCalculations
        public async Task<ActionResult> Index()
        {
            var tblTeamCapacityCalculations = db.tblTeamCapacityCalculations.Include(t => t.tblOrder).Include(t => t.tblUserGroupCompany);
            return View(await tblTeamCapacityCalculations.ToListAsync());
        }
        public async Task<JsonResult> GetTeamCapacity(int ServiceProviderId=0,string FromDate="",string Todate="")
        {
            var objResult = new vm_Result();
            try
            {
                db_Settings objSettings = new db_Settings();
                var providerid = (int)Session[cls_Defaults.Session_UserGroupId];
                //var tblProviderWorkinHours = db.tblTeamCapacities.Where(z => z.ServiceProviderId == providerid).Include(t => t.tblUserGroupCompany);

                //var providerundersupp=db.tblProviderSettingMappers.Where(c=>c.SupplierId==providerid).ToList();
                int userGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);

                var splist = objSettings.GetProviderSettingID(userGroupTypeId);

                var tbteamcapacity = db.tblTeamCapacityCalculations.Where(x => splist.Contains(x.ServiceProviderId)).Include(t => t.tblOrder).Include(t => t.tblUserGroupCompany).OrderByDescending(x => x.Id);

                IQueryable<tblTeamCapacityCalculation> temcaapcityobj = db.tblTeamCapacityCalculations;


                if (ServiceProviderId != null && ServiceProviderId > 0)
                {
                    temcaapcityobj = temcaapcityobj.Where(x => x.ServiceProviderId >= ServiceProviderId);
                }

                if (!string.IsNullOrEmpty(FromDate))
                {
                    DateTime stdt = Convert.ToDateTime(FromDate);
                    temcaapcityobj = temcaapcityobj.Where(x => x.InstallDate >= stdt);
                }
                if (!string.IsNullOrEmpty(Todate))
                {
                    DateTime endt = Convert.ToDateTime(Todate);
                    temcaapcityobj = temcaapcityobj.Where(x => x.InstallDate <= endt);
                }


                List<vm_tblTeamCapacity> modelList = new List<vm_tblTeamCapacity>();
                vm_tblTeamCapacity model = new vm_tblTeamCapacity();
                foreach (var item in temcaapcityobj)
                {
                    model = new vm_tblTeamCapacity();
                    model.Id = item.Id;
                    if (item.InstallDate == null)
                    {
                        model.InstallDate = null;
                    }
                    else
                    {
                        model.InstallDate = item.InstallDate;
                    }
                    model.DailyCapacity = item.DailyCapacity < 0 ? 0 : item.DailyCapacity;
                    model.ConsumedCapacity = item.ConsumedCapacity < 0 ? 0 : item.ConsumedCapacity;
                    model.CurrentCapacity = item.CurrentCapacity < 0 ? 0 : item.CurrentCapacity;
                    model.CapcityPercentage = item.CapcityPercentage < 0 ? 0 : item.CapcityPercentage;
                    if (item.Updatedate == null)
                    {
                        model.Updatedate = null;
                    }
                    else
                    {
                        model.Updatedate = item.Updatedate.Value;
                    }
                    if (item.tblOrder == null)
                    {
                        model.tblOrder = null ;
                    }
                    else
                    {
                        model.tblOrder = item.tblOrder.SellerName;
                    }
                    model.tblUserGroupCompany = item.tblUserGroupCompany.CompanyNameEN;
                    model.ServiceProviderId = item.ServiceProviderId;
                    modelList.Add(model);
                }


                objResult.Data = modelList;
                objResult.Count = modelList.Count;
            
            }
            catch (Exception ex)
            { 
            }
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
        // GET: tblTeamCapacityCalculations/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblTeamCapacityCalculation tblTeamCapacityCalculation = await db.tblTeamCapacityCalculations.FindAsync(id);
            if (tblTeamCapacityCalculation == null)
            {
                return HttpNotFound();
            }
            return View(tblTeamCapacityCalculation);
        }

        // GET: tblTeamCapacityCalculations/Create
        public ActionResult Create()
        {
            ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName");
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN");
            return View();
        }

        // POST: tblTeamCapacityCalculations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,OrderId,InstallDate,ServiceProviderId,DailyCapacity,ConsumedCapacity,CurrentCapacity,CapcityPercentage,Updatedate")] tblTeamCapacityCalculation tblTeamCapacityCalculation)
        {
            if (ModelState.IsValid)
            {
                db.tblTeamCapacityCalculations.Add(tblTeamCapacityCalculation);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName", tblTeamCapacityCalculation.OrderId);
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacityCalculation.ServiceProviderId);
            return View(tblTeamCapacityCalculation);
        }

        // GET: tblTeamCapacityCalculations/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblTeamCapacityCalculation tblTeamCapacityCalculation = await db.tblTeamCapacityCalculations.FindAsync(id);
            if (tblTeamCapacityCalculation == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName", tblTeamCapacityCalculation.OrderId);
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacityCalculation.ServiceProviderId);
            return View(tblTeamCapacityCalculation);
        }

        // POST: tblTeamCapacityCalculations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,OrderId,InstallDate,ServiceProviderId,DailyCapacity,ConsumedCapacity,CurrentCapacity,CapcityPercentage,Updatedate")] tblTeamCapacityCalculation tblTeamCapacityCalculation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblTeamCapacityCalculation).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName", tblTeamCapacityCalculation.OrderId);
            ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacityCalculation.ServiceProviderId);
            return View(tblTeamCapacityCalculation);
        }

        // GET: tblTeamCapacityCalculations/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblTeamCapacityCalculation tblTeamCapacityCalculation = await db.tblTeamCapacityCalculations.FindAsync(id);
            if (tblTeamCapacityCalculation == null)
            {
                return HttpNotFound();
            }
            return View(tblTeamCapacityCalculation);
        }

        // POST: tblTeamCapacityCalculations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            tblTeamCapacityCalculation tblTeamCapacityCalculation = await db.tblTeamCapacityCalculations.FindAsync(id);
            db.tblTeamCapacityCalculations.Remove(tblTeamCapacityCalculation);
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

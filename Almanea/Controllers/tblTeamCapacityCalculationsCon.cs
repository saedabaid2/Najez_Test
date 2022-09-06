using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;

namespace Almanea.Controllers
{

	public class tblTeamCapacityCalculationsController : Controller
	{
		private AlmaneaDbEntities db = new AlmaneaDbEntities();

		public async Task<ActionResult> Index()
		{
			IQueryable<tblTeamCapacityCalculation> tblTeamCapacityCalculations = db.tblTeamCapacityCalculations.Include((tblTeamCapacityCalculation t) => t.tblOrder).Include((tblTeamCapacityCalculation t) => t.tblUserGroupCompany);
			return View(await tblTeamCapacityCalculations.ToListAsync());
		}

		public async Task<JsonResult> GetTeamCapacity(int ServiceProviderId = 0, string FromDate = "", string Todate = "")
		{
			vm_Result objResult = new vm_Result();
			try
			{
				db_Settings objSettings = new db_Settings();
				_ = (int)base.Session[cls_Defaults.Session_UserGroupId];
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				List<int?> splist = objSettings.GetProviderSettingID(userGroupTypeId);
				_ = from x in db.tblTeamCapacityCalculations.Where((tblTeamCapacityCalculation x) => splist.Contains(x.ServiceProviderId)).Include((tblTeamCapacityCalculation t) => t.tblOrder).Include((tblTeamCapacityCalculation t) => t.tblUserGroupCompany)
					orderby x.Id descending
					select x;
				IQueryable<tblTeamCapacityCalculation> temcaapcityobj = db.tblTeamCapacityCalculations;
				if (ServiceProviderId > 0)
				{
					temcaapcityobj = temcaapcityobj.Where((tblTeamCapacityCalculation x) => x.ServiceProviderId >= (int?)ServiceProviderId);
				}
				if (!string.IsNullOrEmpty(FromDate))
				{
					DateTime stdt = Convert.ToDateTime(FromDate);
					temcaapcityobj = temcaapcityobj.Where((tblTeamCapacityCalculation x) => x.InstallDate >= stdt);
				}
				if (!string.IsNullOrEmpty(Todate))
				{
					DateTime endt = Convert.ToDateTime(Todate);
					temcaapcityobj = temcaapcityobj.Where((tblTeamCapacityCalculation x) => x.InstallDate <= endt);
				}
				List<vm_tblTeamCapacity> modelList = new List<vm_tblTeamCapacity>();
				new vm_tblTeamCapacity();
				foreach (tblTeamCapacityCalculation item in temcaapcityobj)
				{
					vm_tblTeamCapacity model = new vm_tblTeamCapacity();
					model.Id = item.Id;
					if (!item.InstallDate.HasValue)
					{
						model.InstallDate = null;
					}
					else
					{
						model.InstallDate = item.InstallDate;
					}
					model.DailyCapacity = ((item.DailyCapacity < 0) ? new int?(0) : item.DailyCapacity);
					model.ConsumedCapacity = ((item.ConsumedCapacity < 0) ? new int?(0) : item.ConsumedCapacity);
					model.CurrentCapacity = ((item.CurrentCapacity < 0) ? new int?(0) : item.CurrentCapacity);
					decimal? capcityPercentage = item.CapcityPercentage;
					model.CapcityPercentage = (((capcityPercentage.GetValueOrDefault() < default(decimal)) & capcityPercentage.HasValue) ? new decimal?(default(decimal)) : item.CapcityPercentage);
					if (!item.Updatedate.HasValue)
					{
						model.Updatedate = null;
					}
					else
					{
						model.Updatedate = item.Updatedate.Value;
					}
					if (item.tblOrder == null)
					{
						model.tblOrder = null;
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
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = 0,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> Details(int? id)
		{
			if (!id.HasValue)
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

		public ActionResult Create()
		{
			base.ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName");
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "Id,OrderId,InstallDate,ServiceProviderId,DailyCapacity,ConsumedCapacity,CurrentCapacity,CapcityPercentage,Updatedate")] tblTeamCapacityCalculation tblTeamCapacityCalculation)
		{
			if (base.ModelState.IsValid)
			{
				db.tblTeamCapacityCalculations.Add(tblTeamCapacityCalculation);
				await db.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			base.ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName", tblTeamCapacityCalculation.OrderId);
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacityCalculation.ServiceProviderId);
			return View(tblTeamCapacityCalculation);
		}

		public async Task<ActionResult> Edit(int? id)
		{
			if (!id.HasValue)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			tblTeamCapacityCalculation tblTeamCapacityCalculation = await db.tblTeamCapacityCalculations.FindAsync(id);
			if (tblTeamCapacityCalculation == null)
			{
				return HttpNotFound();
			}
			base.ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName", tblTeamCapacityCalculation.OrderId);
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacityCalculation.ServiceProviderId);
			return View(tblTeamCapacityCalculation);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "Id,OrderId,InstallDate,ServiceProviderId,DailyCapacity,ConsumedCapacity,CurrentCapacity,CapcityPercentage,Updatedate")] tblTeamCapacityCalculation tblTeamCapacityCalculation)
		{
			if (base.ModelState.IsValid)
			{
				db.Entry(tblTeamCapacityCalculation).State = EntityState.Modified;
				await db.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			base.ViewBag.OrderId = new SelectList(db.tblOrders, "OrderId", "SellerName", tblTeamCapacityCalculation.OrderId);
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacityCalculation.ServiceProviderId);
			return View(tblTeamCapacityCalculation);
		}

		public async Task<ActionResult> Delete(int? id)
		{
			if (!id.HasValue)
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

		[HttpPost]
		[ActionName("Delete")]
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
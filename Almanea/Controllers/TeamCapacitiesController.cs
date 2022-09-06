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
using EntityFrameworkPaginate;

namespace Almanea.Controllers
{

	public class TeamCapacitiesController : BaseController
	{
		private AlmaneaDbEntities db = new AlmaneaDbEntities();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult GetTeamCapacity(vm_JqueryDataTables dmodel, int ServiceProviderId = 0, string StartDate = "", string EndDate = "")
		{
			vm_Result objResult = new vm_Result();
			try
			{
				db_Settings objSettings = new db_Settings();
				int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				List<int?> splist = new List<int?>();
				if (userGroupTypeId == 1 && (accountTypeId == 20 || accountTypeId == 6))
				{
					ServiceProviderId = userGroupId;
					splist.Add(userGroupId);
				}
				if (userGroupTypeId == 2 && (accountTypeId == 17 || accountTypeId == 14))
				{
					splist = objSettings.GetProviderSettingID(userGroupId);
				}
				int PageNo = 1;
				if (dmodel.iDisplayStart >= dmodel.iDisplayLength)
				{
					PageNo = dmodel.iDisplayStart / dmodel.iDisplayLength + 1;
				}
				Sorts<tblTeamCapacityCalculation> sorts = new Sorts<tblTeamCapacityCalculation>();
				Filters<tblTeamCapacityCalculation> filters = new Filters<tblTeamCapacityCalculation>();
				if (ServiceProviderId > 0)
				{
					filters.Add(ServiceProviderId > 0, (tblTeamCapacityCalculation x) => x.ServiceProviderId == (int?)ServiceProviderId);
				}
				if (!string.IsNullOrEmpty(StartDate))
				{
					DateTime stdt = Convert.ToDateTime(StartDate);
					filters.Add(!string.IsNullOrEmpty(StartDate), (tblTeamCapacityCalculation x) => DbFunctions.TruncateTime(x.InstallDate) >= DbFunctions.TruncateTime(stdt));
				}
				if (!string.IsNullOrEmpty(EndDate))
				{
					DateTime endt = Convert.ToDateTime(EndDate);
					filters.Add(!string.IsNullOrEmpty(EndDate), (tblTeamCapacityCalculation x) => DbFunctions.TruncateTime(x.InstallDate) <= DbFunctions.TruncateTime(endt));
				}
				sorts.Add(dmodel.iSortCol_0 == 0, (tblTeamCapacityCalculation x) => x.InstallDate, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 1, (tblTeamCapacityCalculation x) => x.DailyCapacity, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 2, (tblTeamCapacityCalculation x) => x.ConsumedCapacity, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 3, (tblTeamCapacityCalculation x) => x.CurrentCapacity, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 4, (tblTeamCapacityCalculation x) => x.CapcityPercentage, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 5, (tblTeamCapacityCalculation x) => x.Updatedate, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 6, (tblTeamCapacityCalculation x) => x.tblOrder, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(dmodel.iSortCol_0 == 7, (tblTeamCapacityCalculation x) => x.tblUserGroupCompany, (!dmodel.sSortDir_0.Equals("asc")) ? true : false);
				Page<tblTeamCapacityCalculation> result = objSettings.GetTeamCapacity(dmodel.iDisplayLength, PageNo, sorts, filters, splist);
				List<tblTeamCapacityCalculation> lst = result.Results.ToList();
				List<vm_tblTeamCapacity> modelList = new List<vm_tblTeamCapacity>();
				vm_tblTeamCapacity model = new vm_tblTeamCapacity();
				foreach (tblTeamCapacityCalculation item in lst)
				{
					model = new vm_tblTeamCapacity();
					model.Id = item.Id;
					if (!item.InstallDate.HasValue)
					{
						model.InstallDate = null;
					}
					else
					{
						model.InstallDate = Convert.ToDateTime(item.InstallDate);
					}
					model.DailyCapacity = ((item.DailyCapacity < 0) ? new int?(0) : item.DailyCapacity);
					model.ConsumedCapacity = ((item.ConsumedCapacity < 0) ? new int?(0) : item.ConsumedCapacity);
					model.CurrentCapacity = ((item.CurrentCapacity < 0) ? new int?(0) : item.CurrentCapacity);
					vm_tblTeamCapacity vm_tblTeamCapacity = model;
					decimal? capcityPercentage = item.CapcityPercentage;
					vm_tblTeamCapacity.CapcityPercentage = (((capcityPercentage.GetValueOrDefault() < default(decimal)) & capcityPercentage.HasValue) ? new decimal?(default(decimal)) : item.CapcityPercentage);
					if (!item.Updatedate.HasValue)
					{
						model.Updatedate = null;
					}
					else
					{
						model.Updatedate = item.Updatedate.Value.AddHours(0.0);
					}
					if (item.tblOrder == null)
					{
						model.tblOrder = null;
					}
					else
					{
						model.tblOrder = item.tblOrder.SellerName;
					}
					model.InstallDate = ((!model.InstallDate.HasValue) ? new DateTime?(DateTime.UtcNow) : model.InstallDate);
					model.tblUserGroupCompany = item.tblUserGroupCompany.CompanyNameEN;
					model.ServiceProviderId = item.ServiceProviderId;
					modelList.Add(model);
				}
				objResult.Data = modelList;
				objResult.Count = result.RecordCount;
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

		public async Task<int> EditUser(vm_User model)
		{
			tblAdminUser group = await db.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == model.UserId);
			group.AccountTypeId = model.AccountTypeId;
			group.UserGroupTypeId = model.UserGroupTypeId;
			group.LabourIsDriver = model.LabourIsDriver;
			group.Status = model.StatusId;
			await db.SaveChangesAsync();
			return model.UserId;
		}

		[HttpGet]
		public object Updatedailcapacity()
		{
			db_User objUser = new db_User();
			db_Settings objSettings = new db_Settings();
			int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
			List<tblLaborInactive> blockdate = db.tblLaborInactives.ToList();
			if (blockdate.Count > 0)
			{
				tblLaborInactive spbldates;
				foreach (tblLaborInactive item in blockdate)
				{
					spbldates = item;
					int Index = 0;
					List<DateTime> bdate = spbldates.InactiveDates.Split(',').Select(Convert.ToDateTime).ToList();
					foreach (DateTime itemblovkdate in bdate)
					{
						_ = itemblovkdate.Date;
						string ss = itemblovkdate.Date.ToString().Substring(0, 10);
						string previusdat = DateTime.UtcNow.AddDays(-1.0).ToString("dd/MM/yyyy");
						string currentdate = DateTime.UtcNow.AddDays(0.0).ToString("dd/MM/yyyy");
						tblAdminUser group = db.tblAdminUsers.Where((tblAdminUser x) => (int?)x.UserId == spbldates.LabourId).FirstOrDefault();
						if (ss == currentdate)
						{
							group.UserId = Convert.ToInt32(spbldates.LabourId);
							group.Status = false;
							db.Entry(group).State = EntityState.Modified;
							db.SaveChanges();
							EditCapacity(spbldates.ProviderId, status: false);
						}
						else if (!group.Status && ss == previusdat)
						{
							group.UserId = Convert.ToInt32(spbldates.LabourId);
							group.Status = true;
							db.Entry(group).State = EntityState.Modified;
							db.SaveChanges();
							EditCapacity(spbldates.ProviderId, status: true);
						}
					}
				}
			}
			base.ViewBag.UpdateCap = "Updated Capacity";
			return "Updated Capacity";
		}

		public int EditCapacity(int? providerId, bool status)
		{
			db_User objUser = new db_User();
			db_Settings objSettings = new db_Settings();
			List<tblTeamCapacityCalculation> model = db.tblTeamCapacityCalculations.Where((tblTeamCapacityCalculation x) => x.ServiceProviderId == providerId && x.InstallDate >= DateTime.UtcNow).ToList();
			tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(providerId);
			int dailycap = Convert.ToInt32(wokrhrs.KeyValue);
			if (model.Count > 0)
			{
				foreach (tblTeamCapacityCalculation item in model)
				{
					if (!status)
					{
						item.DailyCapacity = Convert.ToInt32(item.DailyCapacity) - dailycap;
						item.CurrentCapacity = Convert.ToInt32(item.CurrentCapacity) - dailycap;
						double curavailabe2 = (double)item.CurrentCapacity.Value / (double)item.DailyCapacity.Value * 100.0;
						item.CapcityPercentage = Convert.ToDecimal(curavailabe2);
					}
					if (status)
					{
						item.DailyCapacity = Convert.ToInt32(item.DailyCapacity) + dailycap;
						item.CurrentCapacity = Convert.ToInt32(item.CurrentCapacity) + dailycap;
						double curavailabe = (double)item.CurrentCapacity.Value / (double)item.DailyCapacity.Value * 100.0;
						item.CapcityPercentage = Convert.ToDecimal(curavailabe);
					}
					item.ServiceProviderId = providerId;
					item.Updatedate = DateTime.UtcNow;
					db.Entry(item).State = EntityState.Modified;
					db.SaveChanges();
				}
			}
			return 1;
		}

		public async Task<ActionResult> Details(int? id)
		{
			if (!id.HasValue)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			tblTeamCapacity tblTeamCapacity = await db.tblTeamCapacities.FindAsync(id);
			if (tblTeamCapacity == null)
			{
				return HttpNotFound();
			}
			return View(tblTeamCapacity);
		}

		public ActionResult Create()
		{
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create([Bind(Include = "Id,ServiceProviderId,DailyCapacity,ConsumedCapacity,Updatedate,CurrentCapacity")] tblTeamCapacity tblTeamCapacity)
		{
			if (base.ModelState.IsValid)
			{
				db.tblTeamCapacities.Add(tblTeamCapacity);
				await db.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacity.ServiceProviderId);
			return View(tblTeamCapacity);
		}

		public async Task<ActionResult> Edit(int? id)
		{
			if (!id.HasValue)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			tblTeamCapacity tblTeamCapacity = await db.tblTeamCapacities.FindAsync(id);
			if (tblTeamCapacity == null)
			{
				return HttpNotFound();
			}
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacity.ServiceProviderId);
			return View(tblTeamCapacity);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit([Bind(Include = "Id,ServiceProviderId,DailyCapacity,ConsumedCapacity,Updatedate,CurrentCapacity")] tblTeamCapacity tblTeamCapacity)
		{
			if (base.ModelState.IsValid)
			{
				db.Entry(tblTeamCapacity).State = EntityState.Modified;
				await db.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", tblTeamCapacity.ServiceProviderId);
			return View(tblTeamCapacity);
		}

		public async Task<ActionResult> Delete(int? id)
		{
			if (!id.HasValue)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			tblTeamCapacity tblTeamCapacity = await db.tblTeamCapacities.FindAsync(id);
			if (tblTeamCapacity == null)
			{
				return HttpNotFound();
			}
			return View(tblTeamCapacity);
		}

		[HttpPost]
		[ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmed(int id)
		{
			tblTeamCapacity tblTeamCapacity = await db.tblTeamCapacities.FindAsync(id);
			db.tblTeamCapacities.Remove(tblTeamCapacity);
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
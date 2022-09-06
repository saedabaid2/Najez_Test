using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Mvc;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;
using Microsoft.CSharp.RuntimeBinder;

namespace Almanea.Controllers
{

	public class SettingController : BaseController
	{
		private db_Settings objSettings = new db_Settings();

		private bool isEnglish = cls_Defaults.IsEnglish;

		private AlmaneaDbEntities db = new AlmaneaDbEntities();

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public async Task<ActionResult> Index()
		{
			if (base.Session[cls_Defaults.Session_UserGroupId] != null)
			{
				int providerid = (int)base.Session[cls_Defaults.Session_UserGroupId];
				string ProviderWorkinHour = (from s in db.tblSettings
											 where s.KeyName == "WorkingHours" && s.ProviderId == providerid
											 select s.KeyValue).FirstOrDefault();
				if (ProviderWorkinHour != null)
				{
					base.ViewBag.ServiceProviderId = new SelectList(db.tblUserGroupCompanies, "UserGroupId", "CompanyNameEN", providerid.ToString());
					base.ViewBag.ProviderWorkinHour = ProviderWorkinHour;
				}
				string ProviderTeamCapacityPercentage = (from s in db.tblSettings
														 where s.KeyName == "TeamCapacityPercentage" && s.ProviderId == providerid
														 select s.KeyValue).FirstOrDefault();
				if (ProviderTeamCapacityPercentage != null)
				{
					base.ViewBag.TeamCapacityPercentage = ProviderTeamCapacityPercentage;
				}
				string ProviderBlockDate = (from s in db.tblSettings
											where s.KeyName == "BlockDate" && s.ProviderId == providerid
											select s.KeyValue).FirstOrDefault();
				if (ProviderBlockDate == null)
				{
					base.ViewBag.BlockDate = null;
				}
				else
				{
					base.ViewBag.BlockDate = ProviderBlockDate;
				}
				int? HasInventory = (from s in db.tblSettings
									 where s.KeyName == "TeamCapacityPercentage" && s.ProviderId == providerid
									 select s.HasInventory).FirstOrDefault();
				int? HasNotify = (from s in db.tblSettings
								  where s.KeyName == "TeamCapacityPercentage" && s.ProviderId == providerid
								  select s.notify).FirstOrDefault();
				if (!HasInventory.HasValue || HasInventory != 1)
				{
					base.ViewBag.HasInventory = 0;
				}
				else
				{
					base.ViewBag.HasInventory = 1;
				}
				if (!HasNotify.HasValue || HasNotify != 1)
				{
					base.ViewBag.HasNotify = 0;
				}
				else
				{
					base.ViewBag.HasNotify = 1;
				}
			}
			return View(await objSettings.GetSetting());
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public ActionResult SupplierSetting()
		{
			vm_ProviderSettings model = objSettings.GetSPSetting();
			return View(model);
		}

		[HttpPost]
		public async Task<JsonResult> ProviderSettingadd(vm_ProviderSettings model, int?[] basic)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				if (!model.IsInternal)
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddProviderMap(model, basic);
				}
				else
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddProviderMapInternal(model, basic);
				}
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public async Task<JsonResult> Edit(vm_Settings model, string workingHours, int? TeamCapacityPercentage, string HasInventory = "")
		{
			//, string notify = ""
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				bool hasInventory = ((HasInventory == "on") ? true : false);
				//bool notifyme = ((notify == "on") ? true : false);
				int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				if (UserGroupTypeId == 1 && (AccountTypeId == 20 || AccountTypeId == 6))
				{
					int providerid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
					new tblProviderWorkinHour();
					int WorkingHours = 0;
					if (workingHours != null)
					{
						WorkingHours = Convert.ToInt32(workingHours);
					}
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditProviderSetting(model, TeamCapacityPercentage, WorkingHours, hasInventory, false);
					if (output.StatusId == 1)
					{
						UpdateCapacity(providerid, TeamCapacityPercentage);
					}
				}
				if (UserGroupTypeId == 8)
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditSetting(model);
				}
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		public int UpdateCapacity(int? providerId, int? workinhhour)
		{
			db_User objUser = new db_User();
			db_Settings objSettings = new db_Settings();
			tblTeamCapacity tblTeamCapacity2 = db.tblTeamCapacities.Where((tblTeamCapacity x) => x.ServiceProviderId == providerId).FirstOrDefault();
			tblUserGroupCompany addebysuppl = db.tblUserGroupCompanies.Where((tblUserGroupCompany c) => (int?)c.UserGroupId == providerId).FirstOrDefault();
			int cap = 0;
			List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(providerId));
			if (labours.Count > 0)
			{
				cap = labours.Count * Convert.ToInt32(workinhhour);
			}
			if (tblTeamCapacity2 != null)
			{
				tblTeamCapacity2.DailyCapacity = cap;
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

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public ActionResult Location()
		{
			vm_Locations vm = new vm_Locations();
			vm.DirectionList = objSettings.GetDirection().ToList();
			return View(vm);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public ActionResult DirectioJson()
		{
			List<vm_Direction> DirectionList = objSettings.GetDirection().ToList();
			return Json(new
			{
				data = DirectionList
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public JsonResult GetLocations(vm_JqueryDataTables model)
		{
			vm_Result objResult = new vm_Result();
			List<vm_Direction> DirectionList = new List<vm_Direction>();
			try
			{
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Sorts<tblLocation> sorts = new Sorts<tblLocation>();
				sorts.Add(model.iSortCol_0 == 0, (tblLocation x) => x.LocationNameEN, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 5, (tblLocation x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				object UserGroupId = base.Session[cls_Defaults.Session_UserGroupId];
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				Page<tblLocation> result = new Page<tblLocation>();
				result = ((userGroupTypeId != 2) ? objSettings.GetLocations(model.iDisplayLength, PageNo, sorts) : objSettings.GetSupllierAdminLocations(model.iDisplayLength, PageNo, sorts, UserGroupId.ToString()));
				List<tblLocation> lst = result.Results.ToList();
				List<vm_Locations> output = Mapper.Map<List<tblLocation>, List<vm_Locations>>(lst);
				base.ViewBag.directionList = from f in objSettings.GetDirection()
											 select new SelectListItem
											 {
												 Value = f.Id.ToString(),
												 Text = f.DirectionName
											 };
				objResult.Data = output;
				objResult.Count = result.RecordCount;
				DirectionList = objSettings.GetDirection().ToList();
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public async Task<JsonResult> AddEditLocation(vm_Locations model)
		{
			model.UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]).ToString();
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				if (model.LocationId > 0)
				{
					if (string.IsNullOrEmpty(model.LocationNameEN) || string.IsNullOrEmpty(model.LocationNameAR))
					{
						output.Message = Translation.ReqAll;
						return Json(output);
					}
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditLocation(model);
				}
				else
				{
					output.StatusId = objSettings.AddLocation(model);
				}
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public ActionResult Services()
		{
			return View();
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public ActionResult GetServices(vm_JqueryDataTables model)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int userId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				int CurrentUserId = (int)base.Session[cls_Defaults.Session_UserId];
				Filters<tblService> filters = new Filters<tblService>();
				Sorts<tblService> sorts = new Sorts<tblService>();
				if (userGroupTypeId == 2 && accountTypeId == 14)
				{
					int SupplierId = objSettings.GetSupplierOrProviderAdminId(CurrentUserId);
					filters.Add(CurrentUserId > 0, (tblService x) => x.UserId == SupplierId);
				}
				else
				{
					filters.Add(CurrentUserId > 0, (tblService x) => x.UserId == CurrentUserId);
				}
				sorts.Add(model.iSortCol_0 == 0, (tblService x) => x.ServiceNameEN, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblService x) => x.ServiceNameAR, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 5, (tblService x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				Page<tblService> result = new Page<tblService>();
				result = objSettings.GetServices(model.iDisplayLength, PageNo, sorts, filters);
				List<tblService> lst = result.Results.OrderBy((tblService l) => l.ServiceId).ToList();
				List<vm_Services> output = Mapper.Map<List<tblService>, List<vm_Services>>(lst);
				foreach (vm_Services item in output)
				{
					if (isEnglish)
					{
						item.ServiceName = item.ServiceNameEN;
					}
					else
					{
						item.ServiceName = item.ServiceNameAR;
					}
				}
				objResult.Data = output;
				objResult.Count = result.RecordCount;
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public async Task<ActionResult> EditService(string Id)
		{
			vm_Services output = new vm_Services();
			try
			{
				int id = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
				output = Mapper.Map<tblService, vm_Services>(await objSettings.GetServicesById(id));
				int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				new List<Category>();
				new List<SelectListItem>();
				List<SelectListItem> AccountType = ((userGroupTypeId != 2 || accountTypeId != 17) ? objSettings.GetCategoryList() : objSettings.GetSupplierAdminCategoryList(UserGroupId));
				base.ViewBag.CategoryIds = AccountType;
				base.ViewBag.CategoryId = output.CategoryId;
			}
			catch (Exception)
			{
			}
			return View(output);
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public async Task<ActionResult> UpdateService(vm_Services model)
		{
			model.SupplierId = base.Session[cls_Defaults.Session_UserGroupId].ToString();
			vm_jsOutput output = new vm_jsOutput();
			model.ServiceId = Convert.ToInt32(EncryptDecrypt.Decrypt(model.EncryptId));
			try
			{
				if (model.ServiceId > 0)
				{
					if (string.IsNullOrEmpty(model.ServiceNameEN) || string.IsNullOrEmpty(model.ServiceNameAR) || model.UnitPrice == 0m)
					{
						output.Message = Translation.ReqAll;
					}
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditService2(model);
				}
				else
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddService(model);
					model.ServiceId = output.StatusId;
				}
				objSettings.InserOrUpdateServiceMapper(model);
			}
			catch (Exception)
			{
			}
			return RedirectToAction("Services");
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
		public async Task<JsonResult> AddEditService(vm_Services model)
		{
			model.SupplierId = base.Session[cls_Defaults.Session_UserGroupId].ToString();
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				if (model.ServiceId > 0)
				{
					if (string.IsNullOrEmpty(model.ServiceNameEN) || string.IsNullOrEmpty(model.ServiceNameAR) || model.UnitPrice == 0m)
					{
						output.Message = Translation.ReqAll;
						return Json(output);
					}
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditService(model);
				}
				else
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddService(model);
					model.ServiceId = output.StatusId;
				}
				objSettings.InserOrUpdateServiceMapper(model);
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public ActionResult AdditionalWork()
		{
			return View();
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public ActionResult GetAdditionalWork(vm_JqueryDataTables model)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Sorts<tblAdditionalWork> sorts = new Sorts<tblAdditionalWork>();
				sorts.Add(model.iSortCol_0 == 0, (tblAdditionalWork x) => x.AdditionalWorkNameEN, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblAdditionalWork x) => x.AdditionalWorkNameAR, (!model.sSortDir_0.Equals("asc")) ? true : false);
				int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				Page<tblAdditionalWork> result = new Page<tblAdditionalWork>();
				result = ((userGroupTypeId != 2 || accountTypeId != 17) ? objSettings.GetAdditionalWork(model.iDisplayLength, PageNo, sorts) : objSettings.GetSupplierAdminAdditionalWork(model.iDisplayLength, PageNo, sorts, UserGroupId));
				List<tblAdditionalWork> lst = result.Results.OrderBy((tblAdditionalWork l) => l.AdditionalWorkId).ToList();
				List<vm_AdditionalWork> output = (List<vm_AdditionalWork>)(objResult.Data = Mapper.Map<List<tblAdditionalWork>, List<vm_AdditionalWork>>(lst));
				objResult.Count = result.RecordCount;
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public async Task<ActionResult> EditAdditionalWork(string Id)
		{
			vm_AdditionalWork output = new vm_AdditionalWork();
			try
			{
				output = Mapper.Map<tblAdditionalWork, vm_AdditionalWork>(await objSettings.GetAdditionalWorkById(Convert.ToInt32(Id)));
				int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				new List<Category>();
				new List<SelectListItem>();
				List<SelectListItem> AccountType = ((userGroupTypeId != 2 || accountTypeId != 17) ? objSettings.GetCategoryList() : objSettings.GetSupplierAdminCategoryList(UserGroupId));
				base.ViewBag.CategoryIds = AccountType;
				base.ViewBag.CategoryId = output.CategoryId;
			}
			catch (Exception)
			{
			}
			return View(output);
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public async Task<ActionResult> UpdateAdditionalWork(vm_AdditionalWork model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				if (model.AdditionalWorkId > 0)
				{
					if (string.IsNullOrEmpty(model.AdditionalWorkNameEN) || string.IsNullOrEmpty(model.AdditionalWorkNameAR) || model.Price == 0m)
					{
						output.Message = Translation.ReqAll;
					}
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditAdditionalWork(model);
				}
				else
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddAdditionalWork(model);
					model.AdditionalWorkId = output.StatusId;
				}
			}
			catch (Exception)
			{
			}
			return RedirectToAction("AdditionalWork");
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public async Task<JsonResult> AddEditAdditionalWork(vm_AdditionalWork model)
		{
			vm_jsOutput output = new vm_jsOutput();
			int UserGroupId = (model.UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]));
			try
			{
				if (model.AdditionalWorkId > 0)
				{
					if (string.IsNullOrEmpty(model.AdditionalWorkNameEN) || string.IsNullOrEmpty(model.AdditionalWorkNameAR) || model.Price == 0m)
					{
						output.Message = Translation.ReqAll;
						return Json(output);
					}
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.EditAdditionalWork(model);
				}
				else
				{
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddAdditionalWork(model);
					model.AdditionalWorkId = output.StatusId;
				}
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier", "Provider" })]
		public ActionResult Orders(int statusId = 0, int supplierId = 0, string date = "", int location = 0)
		{
			base.ViewBag.DirectionList = objSettings.GetDirection().ToList();
			FilterDropDown model = new FilterDropDown();
			model.StatusId = statusId;
			model.SupplierId = supplierId;
			if (date != string.Empty)
			{
				model.Date = date.Substring(0, 10);
			}
			model.LocationId = location;
			return View(model);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier", "Provider" })]
		public async Task<JsonResult> GetOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int StatusId, int UserGroupId, int ServiceId, int supplier, bool delayed = false, bool notupdated = false, int Direction = 0)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrder> filters = new Filters<tblOrder>();
				Sorts<tblOrder> sorts = new Sorts<tblOrder>();
				filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
				filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
				if (ServiceId > 0)
				{
					filters.Add(condition: true, (tblOrder x) => x.tblOrderServices.Any((tblOrderService y) => y.tblService.ServiceId == ServiceId));
				}
				filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
				filters.Add(Direction > 0, (tblOrder x) => x.tblLocation.Direction == (int?)Direction);
				if (StatusId > 0)
				{
					filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId && x.Status != 12 && x.Status != 10);
				}
				else
				{
					filters.Add(condition: true, (tblOrder x) => x.Status != 12 && x.Status != 11 && x.Status != 10);
				}
				if (!string.IsNullOrEmpty(InstallDate))
				{
					InstallDate = InstallDate.Substring(0, 10);
					DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
					filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
				}
				filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
				int mins = Convert.ToInt32(objSettings.GetSettingByKey("ordershowduration"));
				DateTime now5Min = DateTime.Now.AddMinutes(-mins);
				filters.Add(condition: true, (tblOrder x) => x.AddedDate <= now5Min);
				filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
				filters.Add(userGroupId > 0, (tblOrder x) => x.SupplierId == userGroupId);
				if (delayed)
				{
					filters.Add(condition: true, (tblOrder x) => x.InstallDate < DateTime.Now && x.Status != 6 && x.Status != 9 && x.Status != 7);
				}
				if (notupdated)
				{
					filters.Add(condition: true, (tblOrder x) => x.InstallDate <= DateTime.Now && x.Status != 9 && x.Status != 7);
				}
				sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 6, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				new Page<tblOrder>();
				Page<tblOrder> result = objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);
				db_User objUser = new db_User();
				List<tblOrder> lst = result.Results.ToList();
				List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
				DateTime.Now.AddDays(-30.0);
				foreach (vm_OrderList item in output)
				{
					int status = objSettings.GetAssignedStatus(int.Parse(item.OrderNo));
					if (item.ReservedProvider > 0)
					{
						string provider = await objUser.GetGroupName(item.ReservedProvider.Value);
						int orderId = Convert.ToInt32(EncryptDecrypt.Decrypt(item.OrderId));
						item.ReservedBy = await objSettings.GetOrderReservedBy(orderId) + " / " + provider;
					}
					vm_Locations location = objSettings.GetLocation(item.LocationId);
					item.Location = location.LocationNameEN;
					tblProviderTimeSlot timeslot = objSettings.GetTimeslot(int.Parse(item.OrderNo));
					if (timeslot != null)
					{
						item.InstallDate = item.InstallDate.ToString().Substring(0, 10) + "<br/>" + cls_Defaults.get12hour(timeslot.StartHour.Value, timeslot.EndHour.Value);
					}
					else
					{
						item.InstallDate = item.InstallDate.ToString().Substring(0, 10);
					}
					if (status != 0)
					{
						item.StatusText = cls_DropDowns.OrderStatusName(status);
					}
				}
				objResult.Data = output;
				objResult.Count = result.RecordCount;
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier", "Provider" })]
		public ActionResult CalendarOrders(int statusId = 0, int supplierId = 0, string date = "", int location = 0)
		{
			base.ViewBag.DirectionList = objSettings.GetDirection().ToList();
			FilterDropDown model = new FilterDropDown();
			model.StatusId = statusId;
			model.SupplierId = supplierId;
			if (date != string.Empty)
			{
				model.Date = date.Substring(0, 10);
			}
			model.LocationId = location;
			return View(model);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier", "Provider" })]
		public async Task<JsonResult> GetCalendarOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location = 0, int StatusId = 0, int UserGroupId = 0, int ServiceId = 0, int supplier = 0, bool delayed = false, bool notupdated = false, int Direction = 0)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrder> filters = new Filters<tblOrder>();
				Sorts<tblOrder> sorts = new Sorts<tblOrder>();
				filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
				filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
				if (ServiceId > 0)
				{
					filters.Add(condition: true, (tblOrder x) => x.tblOrderServices.Any((tblOrderService y) => y.tblService.ServiceId == ServiceId));
				}
				filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
				filters.Add(Direction > 0, (tblOrder x) => x.tblLocation.Direction == (int?)Direction);
				if (!string.IsNullOrEmpty(InstallDate))
				{
					InstallDate = InstallDate.Substring(0, 10);
					DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
					filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
				}
				filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
				int mins = Convert.ToInt32(objSettings.GetSettingByKey("ordershowduration"));
				DateTime now5Min = DateTime.Now.AddMinutes(-mins);
				filters.Add(condition: true, (tblOrder x) => x.AddedDate <= now5Min);
				filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
				filters.Add(userGroupId > 0, (tblOrder x) => x.SupplierId == userGroupId);
				sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 6, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				new Page<tblOrder>();
				Page<tblOrder> result = objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);
				db_User objUser = new db_User();
				List<tblOrder> lst = result.Results.ToList();
				List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
				DateTime.Now.AddDays(-30.0);
				foreach (vm_OrderList item in output)
				{
					if (item.ReservedProvider > 0)
					{
						string provider = await objUser.GetGroupName(item.ReservedProvider.Value);
						int orderId = Convert.ToInt32(EncryptDecrypt.Decrypt(item.OrderId));
						item.ReservedBy = await objSettings.GetOrderReservedBy(orderId) + " / " + provider;
					}
				}
				objResult.Data = output;
				objResult.Count = result.RecordCount;
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize("Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier")]

		public async Task<ActionResult> OrderDetails(string Id)
		{
			try
			{
				ViewBag.Vat = objSettings.Vat();

				var OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

				var model = await objSettings.GetOrderById(OrderId);
				if (model != null)
				{
					ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);

					ViewBag.History = await objSettings.GetHistory(OrderId);

					return View(model);
				}
				else
					return RedirectToAction("Orders");
			}
			catch (Exception ex) { }
			return HttpNotFound();
		}
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public ActionResult Completed()
		{
			base.ViewBag.Status = 10;
			base.ViewBag.Title = Translation.CompletedOrders;
			base.ViewBag.PageDesc = Translation.CompletedOrderDesc;
			return View();
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public async Task<JsonResult> GetCancelOrDeleteOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId, int UserGroupId = 0, int supplier = 0)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrder> filters = new Filters<tblOrder>();
				Sorts<tblOrder> sorts = new Sorts<tblOrder>();
				filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
				filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
				filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
				if (!string.IsNullOrEmpty(InstallDate))
				{
					DateTime ddt = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
					filters.Add(!string.IsNullOrEmpty(InstallDate), (tblOrder x) => x.InstallDate == ddt);
				}
				filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
				filters.Add(SupplierId > 0, (tblOrder x) => x.SupplierId == SupplierId);
				switch (StatusId)
				{
					case 12:
						filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
						filters.Add(condition: true, (tblOrder x) => x.Status == 12);
						break;
					case 11:
						filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
						filters.Add(condition: true, (tblOrder x) => x.Status == 11);
						break;
					default:
						filters.Add(condition: true, (tblOrder x) => x.Status == 12 || x.Status == 11);
						break;
				}
				if (!string.IsNullOrEmpty(InvoiceNo))
				{
					if (int.TryParse(InvoiceNo, out var i) && i > 1000)
					{
						int orderId2 = Convert.ToInt32(i);
						filters.Add(condition: true, (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId == orderId2);
					}
					else
					{
						filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo));
					}
				}
				sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 6, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				new Page<tblOrder>();
				Page<tblOrder> result = ((userGroupTypeId != 2 || accountype != 17) ? objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters) : objSettings.GetSupplierAdminOrders(model.iDisplayLength, PageNo, sorts, filters, userid));
				List<tblOrder> lst = result.Results.ToList();
				List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
				db_User objUser = new db_User();
				foreach (vm_OrderList item in output)
				{
					if (item.ReservedProvider > 0)
					{
						string provider = await objUser.GetGroupName(item.ReservedProvider.Value);
						int orderId = Convert.ToInt32(EncryptDecrypt.Decrypt(item.OrderId));
						await objSettings.GetOrderReservedBy(orderId);
						item.ReservedBy = provider;
					}
				}
				objResult.Data = output;
				objResult.Count = result.RecordCount;
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier" })]
		public async Task<JsonResult> GetStatusOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId, int UserGroupId = 0, int supplier = 0)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrder> filters = new Filters<tblOrder>();
				Sorts<tblOrder> sorts = new Sorts<tblOrder>();
				filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
				filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
				filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
				if (!string.IsNullOrEmpty(InstallDate))
				{
					DateTime ddt = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
					filters.Add(!string.IsNullOrEmpty(InstallDate), (tblOrder x) => x.InstallDate == ddt);
				}
				filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
				filters.Add(SupplierId > 0, (tblOrder x) => x.SupplierId == SupplierId);
				if (StatusId == 10)
				{
					Status = StatusId;
					filters.Add(condition: true, (tblOrder x) => x.Status == Status);
				}
				if (StatusId == 12)
				{
					filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
					filters.Add(condition: true, (tblOrder x) => x.Status == 12);
				}
				if (StatusId == 11)
				{
					filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
					filters.Add(condition: true, (tblOrder x) => x.Status == 11);
				}
				if (!string.IsNullOrEmpty(InvoiceNo))
				{
					if (int.TryParse(InvoiceNo, out var i) && i > 1000)
					{
						int orderId2 = Convert.ToInt32(i);
						filters.Add(condition: true, (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId == orderId2);
					}
					else
					{
						filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo));
					}
				}
				sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 6, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
				new Page<tblOrder>();
				Page<tblOrder> result = ((userGroupTypeId != 2 || accountype != 17) ? objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters) : objSettings.GetSupplierAdminOrders(model.iDisplayLength, PageNo, sorts, filters, userid));
				List<tblOrder> lst = result.Results.ToList();
				List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
				db_User objUser = new db_User();
				foreach (vm_OrderList item in output)
				{
					if (item.ReservedProvider > 0)
					{
						string provider = await objUser.GetGroupName(item.ReservedProvider.Value);
						int orderId = Convert.ToInt32(EncryptDecrypt.Decrypt(item.OrderId));
						await objSettings.GetOrderReservedBy(orderId);
						item.ReservedBy = provider;
					}
				}
				objResult.Data = output;
				objResult.Count = result.RecordCount;
			}
			catch (Exception)
			{
			}
			return Json(new
			{
				aaData = objResult.Data,
				sEcho = model.sEcho,
				iTotalRecords = objResult.Count,
				iTotalDisplayRecords = objResult.Count
			}, JsonRequestBehavior.AllowGet);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "SellerStaff", "Supplier" })]
		public ActionResult Invoice()
		{
			return View();
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin" })]
		public async Task<ActionResult> Email()
		{
			return View(await objSettings.getEmailById(1));
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin" })]
		public async Task<JsonResult> EditEmail(vm_Email model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objSettings.EditEmail(model);
			}
			catch (Exception)
			{
			}
			return Json(output);
		}


		//SMS
		[SiteAuthorize("Admin", "Executive", "SuperAdmin")]

		public async Task<ActionResult> SMS()
		{
			ViewBag.Data = await objSettings.GetSMS();

			return View();
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin" })]
		public async Task<JsonResult> EditSMS(vm_SMS model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objSettings.EditSMS(model);
			}
			catch (Exception)
			{
			}
			return Json(output);
		}
		//Notification
		[SiteAuthorize("Admin", "Executive", "SuperAdmin")]

		public async Task<ActionResult> PushNotification()
		{
			ViewBag.Data = await objSettings.GetPushNotification();
			return View();
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin" })]
		public async Task<JsonResult> EditPushNotification(vm_PushNotification model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objSettings.EditPushNotification(model);
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin" })]
		public async Task<JsonResult> AddPushNotification(vm_PushNotification model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objSettings.AddPushNotification(model);
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		//Compalin Category
		[SiteAuthorize("Admin", "Executive", "SuperAdmin", "Supplier")]

		public async Task<ActionResult> ComplainCategory()
		{
			int UserGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);
			int AccountTypeId = Convert.ToInt32(Session[cls_Defaults.Session_AccountTypeId]);

			if (UserGroupTypeId == (int)enumGroupType.Supplier && AccountTypeId == (int)enumSupplierAcct.Admin)
			{
				int ProviderId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);
				ViewBag.Data = await objSettings.GetSupplierComplainType(ProviderId);
			}
			else
			{
				ViewBag.Data = await objSettings.GetComplainType();
			}
			return View();
		}
		
		[HttpPost]
		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public async Task<JsonResult> AddEditCategory(vm_ComplainType model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				if (string.IsNullOrEmpty(model.TitleEN) || string.IsNullOrEmpty(model.TitleAR))
				{
					return Json(output);
				}
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objSettings.AddEditComplainType(model);
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
		public ActionResult Cancelled()
		{
			base.ViewBag.Status = 12;
			base.ViewBag.Title = Translation.CompletedOrders;
			base.ViewBag.PageDesc = Translation.CompletedOrderDesc;
			return View();
		}
	}
}
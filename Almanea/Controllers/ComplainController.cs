using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;
using Microsoft.CSharp.RuntimeBinder;

namespace Almanea.Controllers
{

	public class ComplainController : BaseController
	{
		private db_User objUser = new db_User();

		private db_Settings objSettings = new db_Settings();

		[SiteAuthorize(new string[] { "Supplier", "SellerStaff", "User" })]
		[SiteAuthorize("Supplier", "SellerStaff", "User")]
		public async Task<ActionResult> Details(string Id)
		{
			try
			{
				ViewBag.Vat = objSettings.Vat();

				var userGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);
				var userGroupId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);

				var ComplainId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

				var complain = await objSettings.GetComplainById(ComplainId);

				var OrderId = complain.OrderId;

				ViewBag.Id = ComplainId;
				ViewBag.Complain = complain;

				var model = await objSettings.GetOrderById(OrderId);
				if (model != null)
				{
					ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);
					return View(model);
				}
				else
					return RedirectToAction("Index");
			}
			catch (Exception ex) { }
			return HttpNotFound();
		}

		public JsonResult GetComplain(vm_JqueryDataTables model, int OrderId)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrderComplain> filters = new Filters<tblOrderComplain>();
				Sorts<tblOrderComplain> sorts = new Sorts<tblOrderComplain>();
				filters.Add(condition: true, (tblOrderComplain x) => x.OrderId == (int?)OrderId);
				filters.Add(condition: true, (tblOrderComplain x) => x.StatusId == 1);
				sorts.Add(model.iSortCol_0 == 1, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 5, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 0, (tblOrderComplain x) => x.ComplainId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				Page<tblOrderComplain> result = objSettings.GetComplains(model.iDisplayLength, PageNo, sorts, filters, 3);
				List<vm_Complains> output = new List<vm_Complains>();
				List<tblOrderComplain> lst = result.Results.ToList();
				foreach (tblOrderComplain x2 in lst)
				{
					vm_Complains item = new vm_Complains
					{
						Subject = x2.Subject,
						InvoiceNo = x2.tblOrder.InvoiceNo,
						ComplainId = x2.ComplainId,
						AddedOn = x2.AddedOn.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture),
						CloseDate = x2.AddedOn.AddDays(30.0).ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture),
						Comments = x2.Comments,
						Id = EncryptDecrypt.Encrypt(x2.ComplainId.ToString())
					};
					output.Add(item);
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

		[SiteAuthorize(new string[] { "Supplier", "SellerStaff", "User" })]
		public ActionResult Index()
		{
			base.ViewBag.TypeId = 1;
			base.ViewBag.Title = Translation.ComplainList;
			return View();
		}

		[SiteAuthorize(new string[] { "Supplier" })]
		public ActionResult Archieve()
		{
			base.ViewBag.TypeId = 0;
			base.ViewBag.Title = Translation.ArchieveComplain;
			return View("Index");
		}

		public JsonResult GetAllComplain(vm_JqueryDataTables model, string Date, string Category, string InvoiceNo, int TypeId, int status = 0)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrderComplain> filters = new Filters<tblOrderComplain>();
				Sorts<tblOrderComplain> sorts = new Sorts<tblOrderComplain>();
				if (!string.IsNullOrEmpty(Category))
				{
					int complaintypeid = Convert.ToInt32(Category);
					filters.Add(!string.IsNullOrEmpty(Category), (tblOrderComplain x) => x.ComplainTypeId == (int?)complaintypeid);
				}
				if (!string.IsNullOrEmpty(Date))
				{
					DateTime AddedOn = Convert.ToDateTime(Date);
					filters.Add(condition: true, (tblOrderComplain x) => DbFunctions.TruncateTime(x.AddedOn) == DbFunctions.TruncateTime(AddedOn));
				}
				if (!string.IsNullOrEmpty(InvoiceNo))
				{
					int i;
					bool isNumeric = int.TryParse(InvoiceNo, out i);
					int orderId = Convert.ToInt32(i);
					if (isNumeric && i > 1000)
					{
						filters.Add(condition: true, (tblOrderComplain x) => x.tblOrder.InvoiceNo.Equals(InvoiceNo) || x.tblOrder.InvoiceNo.Contains(InvoiceNo) || x.tblOrder.OrderId == orderId);
					}
					else
					{
						filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrderComplain x) => x.tblOrder.InvoiceNo.Equals(InvoiceNo) || x.tblOrder.InvoiceNo.Contains(InvoiceNo) || x.OrderId == (int?)orderId);
					}
				}
				sorts.Add(model.iSortCol_0 == 0, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblOrderComplain x) => x.OrderId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 2, (tblOrderComplain x) => x.AddedBy, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 3, (tblOrderComplain x) => x.ComplainBy, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 4, (tblOrderComplain x) => x.ComplainId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 5, (tblOrderComplain x) => x.StatusId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 6, (tblOrderComplain x) => x.StatusId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				filters.Add(SupplierId > 0, (tblOrderComplain x) => x.tblOrder.SupplierId == SupplierId);
				Page<tblOrderComplain> result = new Page<tblOrderComplain>();
				if (UserGroupTypeId == 2 && (accountype == 14 || accountype == 15))
				{
					int SupplierAdmin = objSettings.GetSupplierOrProviderAdminId(UserId);
					result = objSettings.GetSupplierAdminComplains(model.iDisplayLength, PageNo, sorts, filters, TypeId, SupplierAdmin);
				}
				else
				{
					result = objSettings.GetComplainsforSuppliers(model.iDisplayLength, PageNo, sorts, filters, SupplierId, accountype, UserId);
				}
				List<tblOrderComplain> lst = result.Results.ToList();
				List<vm_ComplainList> output = Mapper.Map<List<tblOrderComplain>, List<vm_ComplainList>>(lst);
				foreach (vm_ComplainList item in output)
				{
					Task<List<string>> list = objSettings.GetmultipleComplaintype(item.ComplainId, IsEnglish);
					if (list.Result != null)
					{
						item.Category = string.Join(", ", list.Result);
					}
					else
					{
						item.Category = (IsEnglish ? item.CategoryEN : item.CategoryAR);
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

		public ActionResult UnAthorized()
		{
			return View();
		}

		public async Task<ActionResult> Create(string Id, byte Type)
		{
			vm_Complain model = new vm_Complain
			{
				OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id)),
				ComplainBy = Type
			};
			List<short> CategoryIdList = objSettings.GetOrderCategoryList(model.OrderId);
			List<SelectListItem> complainType = objSettings.GetComplainList(CategoryIdList);
			new cls_Defaults();
			base.ViewBag.Category = complainType;
			return PartialView("_AddComplain", model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<JsonResult> AddComplain(vm_Complain model)
		{
			string[] array = base.Request.Form["ComplainId"].Split(',');
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				for (int i = 0; i < array.Length; i++)
				{
					model.ComplainId = output.StatusId;
					model.CategoryId = int.Parse(array[i]);
					vm_jsOutput vm_jsOutput = output;
					vm_jsOutput.StatusId = await objSettings.AddComplain(model);
					await objSettings.AddMultipleComplain(model);
				}
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[HttpPost]
		public async Task<JsonResult> UpdateComplain(vm_ComplainResponse model)
		{
			vm_jsOutput output = new vm_jsOutput();
			try
			{
				if (model.StatusId == 0 && string.IsNullOrEmpty(model.Response2))
				{
					output.Message = Translation.EnterComment;
					return Json(output);
				}
				if (model.StatusId == 8)
				{
					model.StatusId = 1;
				}
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objSettings.UpdateComplain(model);
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				if (output.StatusId > 0)
				{
					if (userGroupTypeId == 1)
					{
						output.Data = "/Provider/Complain";
					}
					else
					{
						output.Data = "/Complain/Admin";
					}
				}
			}
			catch (Exception)
			{
			}
			return Json(output);
		}

		[SiteAuthorize(new string[] { "Admin", "SuperAdmin", "Supplier" })]
		public ActionResult Admin()
		{
			base.ViewBag.IsNew = 1;
			base.ViewBag.Title = Translation.ComplainList;
			return View();
		}

		[SiteAuthorize(new string[] { "Admin", "SuperAdmin", "Supplier" })]
		public ActionResult Resolved()
		{
			base.ViewBag.Title = Translation.ArchieveComplain;
			base.ViewBag.IsNew = 0;
			return View("Admin");
		}

		public async Task<JsonResult> ProviderComplain(vm_JqueryDataTables model, int isNew, int CompanyId = 0, int supplier = 0)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
				int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrderComplain> filters = new Filters<tblOrderComplain>();
				Sorts<tblOrderComplain> sorts = new Sorts<tblOrderComplain>();
				filters.Add(CompanyId > 0, (tblOrderComplain x) => x.tblOrder.ReservedProvider == CompanyId);
				filters.Add(supplier > 0, (tblOrderComplain x) => x.tblOrder.SupplierId == supplier);
				sorts.Add(model.iSortCol_0 == 0, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 1, (tblOrderComplain x) => x.OrderId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 2, (tblOrderComplain x) => x.AddedBy, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 3, (tblOrderComplain x) => x.ComplainBy, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 4, (tblOrderComplain x) => x.ComplainId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 5, (tblOrderComplain x) => x.StatusId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 6, (tblOrderComplain x) => x.StatusId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				new Page<tblOrderComplain>();
				Page<tblOrderComplain> result = ((UserGroupTypeId != 2 || AccountTypeId != 17) ? objSettings.GetComplains(model.iDisplayLength, PageNo, sorts, filters, isNew) : objSettings.GetSupplierAdminComplains(model.iDisplayLength, PageNo, sorts, filters, isNew, UserId));
				List<tblOrderComplain> lst = result.Results.ToList();
				List<vm_ComplainList> output = Mapper.Map<List<tblOrderComplain>, List<vm_ComplainList>>(lst);
				db_User objUser = new db_User();
				foreach (vm_ComplainList item in output)
				{
					if (item.Status == "Resolve By Sp")
					{
						item.ResolveOn = (from l in lst
										  where l.ComplainId == item.ComplainId
										  select l.UpdateOn).FirstOrDefault().ToString();
					}
					if (item.ProviderId > 0)
					{
						vm_GroupCompanies groups = await objUser.GetGroupById(item.ProviderId);
						if (groups != null)
						{
							item.Provider = (cls_Defaults.IsEnglish ? groups.CompanyNameEN : groups.CompanyNameAR);
						}
					}
					Task<List<string>> list = objSettings.GetmultipleComplaintype(item.ComplainId, IsEnglish);
					if (list.Result != null)
					{
						item.Category = string.Join(", ", list.Result);
					}
					else
					{
						item.Category = (IsEnglish ? item.CategoryEN : item.CategoryAR);
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

		[SiteAuthorize("Provider", "Admin", "SuperAdmin", "Supplier")]
		public async Task<ActionResult> Preview(string Id)
		{
			try
			{
				bool CanEdit = false;
				ViewBag.Vat = objSettings.Vat();

				var userGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);
				var userGroupId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);

				var ComplainId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

				var complain = await objSettings.GetComplainById(ComplainId);


				var OrderId = complain.OrderId;

				ViewBag.Id = ComplainId;
				ViewBag.Complain = complain;

				var objUser = new db_User();
				var orders = await objUser.GetOrderById(OrderId);
				if (orders != null)
				{
					var model = Mapper.Map<tblOrder, vm_Order>(orders);
					//model.InstallDate = DateTime.Parse(model.InstallDate).ToString("dd/MM/yyyy", cls_Defaults.DateTimeCulture);
					model.InstallDate = DateTime.Parse(model.dtInstallDate.ToString()).ToString("dd/MM/yyyy", cls_Defaults.DateTimeCulture);

					var status = model.Status;
					model.Location = (IsEnglish ? model.LocationEN : model.LocationAR);
					var reservedProvider = model.ReservedProvider;

					if (userGroupTypeId == (byte)enumGroupType.Provider &&
						status >= (byte)OrderStatus.Reserved && reservedProvider != userGroupId)
					{
						TempData["ShowMessage"] = Translation.OrderAlreadyReserved;
						return RedirectToAction("NewOrders");
					}
					else if (userGroupTypeId == (byte)enumGroupType.Provider &&
						status == (byte)OrderStatus.Delete)
					{
						TempData["ShowMessage"] = Translation.OrderDeleted;
						return RedirectToAction("NewOrders");
					}
					else if (userGroupTypeId == (byte)enumGroupType.Provider &&
						model.IsOnEdit == true)
					{
						TempData["ShowMessage"] = Translation.OrderNotReady;
						return RedirectToAction("NewOrders");
					}

					ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);

					var History = await objSettings.GetHistory(OrderId);

					ViewBag.Additional = await objSettings.GetAdditional(OrderId);

					if (userGroupTypeId == (int)enumGroupType.Admin)
					{
						// ProviderInfo                        
						var AddedUser = await objUser.GetUsertbl((int)orders.ReservedBy);
						ViewBag.AddedUser = AddedUser;
					}

					ViewBag.History = History;
					ViewBag.CanEdit = CanEdit;

					return View("ComplainDetail", model);
				}
				else
					return RedirectToAction("Admin");
			}
			catch (Exception ex) { }
			return HttpNotFound();
		}
		public JsonResult ComplainHistory(vm_JqueryDataTables model, int ComplainId)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
				int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblComplainHistory> filters = new Filters<tblComplainHistory>();
				Sorts<tblComplainHistory> sorts = new Sorts<tblComplainHistory>();
				filters.Add(condition: true, (tblComplainHistory x) => x.ComplainId == (int?)ComplainId);
				filters.Add(condition: true, (tblComplainHistory x) => (int?)x.StatusId == (int?)0 || (int?)x.StatusId == (int?)2 || (int?)x.StatusId == (int?)9 || (int?)x.StatusId == (int?)7 || (int?)x.StatusId == (int?)10 || (int?)x.StatusId == (int?)6 || (int?)x.StatusId == (int?)1);
				sorts.Add(model.iSortCol_0 == 1, (tblComplainHistory x) => x.UpdateOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				Page<tblComplainHistory> result = objSettings.GetComplainHistory(model.iDisplayLength, PageNo, sorts, filters);
				List<tblComplainHistory> lst = result.Results.ToList();
				List<vm_ComplainHistory> output = new List<vm_ComplainHistory>();
				db_User objUser = new db_User();
				foreach (tblComplainHistory item in lst)
				{
					byte? tUserGroupTypeId = item.tblAdminUser.UserGroupTypeId;
					int? tUserGroupId = item.tblAdminUser.UserGroupId;
					string addedBy = item.tblAdminUser.FirstName + item.tblAdminUser.LastName;
					output.Add(new vm_ComplainHistory
					{
						AddedOn = item.UpdateOn.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture),
						Comments = item.Comments,
						SendBy = addedBy + ((tUserGroupTypeId == 3) ? " (Admin)" : " (Provider)")
					});
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

		public async Task<ActionResult> Customer(string Id)
		{
			_ = 2;
			try
			{
				string cultureName = "en-US";
				int LinkId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
				tblOrderUserLink userLinks = await objSettings.Get_OrderUserLink(LinkId);
				if (userLinks == null || userLinks.IsActive == false || userLinks.ExpireOn < DateTime.Now)
				{
					return RedirectToAction("UnAthorized");
				}
				vm_Order orders = await objSettings.GetOrderById(userLinks.OrderId.Value);
				if (orders.SmsInArabic)
				{
					cultureName = "ar";
				}
				base.ViewBag.OrderId = Id;
				cultureName = cultureHelper.GetImplementedCulture(cultureName);
				base.Response.Cookies.Remove("Syanah_culture");
				HttpCookie cookie = new HttpCookie("Syanah_culture");
				cookie.Value = cultureName;
				cookie.Expires = DateTime.Now.AddYears(1);
				base.Response.Cookies.Add(cookie);
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureName);
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
				bool IsEnglish = (cls_Defaults.IsEnglish = ((!cultureName.Equals("ar")) ? true : false));
				vm_Complain model = new vm_Complain
				{
					OrderId = userLinks.OrderId.Value
				};
				Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				List<vm_ComplainType> complainType = await objSettings.GetComplainType(orders.SupplierId);
				base.ViewBag.Category = complainType.Select((vm_ComplainType x) => new SelectListItem
				{
					Value = x.ComplainTypeId.ToString(),
					Text = ((cultureName == "en-US") ? x.TitleEN : x.TitleAR)
				}).ToList();
				return View(model);
			}
			catch (Exception)
			{
			}
			return HttpNotFound();
		}

		public async Task<ActionResult> CustomerDetails(string Id, string OrderId)
		{
			try
			{
				int complainId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
				tblOrderComplain output = await objSettings.GetComplainDetail(complainId);
				base.ViewBag.OrderId = OrderId;
				vm_ComplainDetail model = Mapper.Map<tblOrderComplain, vm_ComplainDetail>(output);
				Task<List<string>> list = objSettings.GetmultipleComplaintype(complainId, IsEnglish);
				if (list.Result != null)
				{
					model.Category = string.Join(", ", list.Result);
				}
				else
				{
					model.Category = (IsEnglish ? model.CategoryEN : model.CategoryAR);
				}
				return View(model);
			}
			catch (Exception)
			{
			}
			return HttpNotFound();
		}

		public JsonResult ListCustomer(vm_JqueryDataTables model, int OrderId)
		{
			vm_Result objResult = new vm_Result();
			try
			{
				int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
				int PageNo = 1;
				if (model.iDisplayStart >= model.iDisplayLength)
				{
					PageNo = model.iDisplayStart / model.iDisplayLength + 1;
				}
				Filters<tblOrderComplain> filters = new Filters<tblOrderComplain>();
				Sorts<tblOrderComplain> sorts = new Sorts<tblOrderComplain>();
				filters.Add(condition: true, (tblOrderComplain x) => x.OrderId == (int?)OrderId);
				sorts.Add(model.iSortCol_0 == 1, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 5, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
				sorts.Add(model.iSortCol_0 == 0, (tblOrderComplain x) => x.ComplainId, (!model.sSortDir_0.Equals("asc")) ? true : false);
				Page<tblOrderComplain> result = objSettings.GetComplains(model.iDisplayLength, PageNo, sorts, filters, 3, SupplierId);
				List<tblOrderComplain> lst = result.Results.ToList();
				List<vm_UserComplainList> output = Mapper.Map<List<tblOrderComplain>, List<vm_UserComplainList>>(lst);
				foreach (vm_UserComplainList item in output)
				{
					Task<List<string>> list = objSettings.GetmultipleComplaintype(item.ComplainId, IsEnglish);
					if (list.Result != null)
					{
						item.Category = string.Join(", ", list.Result);
					}
					else
					{
						item.Category = (IsEnglish ? item.CategoryEN : item.CategoryAR);
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
	}
}
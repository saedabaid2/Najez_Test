using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;

namespace Almanea.Controllers
{ 

[SiteAuthorize(new string[] { "Admin", "Provider", "SuperAdmin", "Supplier", "User" })]
public class UserController : BaseController
{
	private db_User objUser = new db_User();

	private AlmaneaDbEntities db = new AlmaneaDbEntities();

	public ActionResult Groups()
	{
		return View();
	}

	public JsonResult GetGroups(vm_JqueryDataTables model, string CompanyName, string Email, string Telephone, int StatusId, int GroupTypeId)
	{
		vm_Result objResult = new vm_Result();
		try
		{
			int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
			int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
			int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
			int UserGroupId2 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
			int PageNo = 1;
			if (model.iDisplayStart >= model.iDisplayLength)
			{
				PageNo = model.iDisplayStart / model.iDisplayLength + 1;
			}
			Filters<tblUserGroupCompany> filters = new Filters<tblUserGroupCompany>();
			Sorts<tblUserGroupCompany> sorts = new Sorts<tblUserGroupCompany>();
			filters.Add(!string.IsNullOrEmpty(CompanyName), (tblUserGroupCompany x) => x.CompanyNameEN.Contains(CompanyName) || x.CompanyNameAR.Contains(CompanyName));
			filters.Add(!string.IsNullOrEmpty(Email), (tblUserGroupCompany x) => x.Email.Equals(Email));
			filters.Add(!string.IsNullOrEmpty(Telephone), (tblUserGroupCompany x) => x.Telephone.Equals(Telephone));
			filters.Add(StatusId > 0, (tblUserGroupCompany x) => x.Status == ((StatusId == 1) ? true : false));
			filters.Add(GroupTypeId > 0, (tblUserGroupCompany x) => x.UserGroupTypeId == GroupTypeId);
			filters.Add(UserId > 0, (tblUserGroupCompany x) => x.AddedBy == UserId);
			sorts.Add(model.iSortCol_0 == 0, (tblUserGroupCompany x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 1, (tblUserGroupCompany x) => x.CompanyNameEN, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 2, (tblUserGroupCompany x) => x.Email, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 3, (tblUserGroupCompany x) => x.Telephone, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 4, (tblUserGroupCompany x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 5, (tblUserGroupCompany x) => x.IsInternal, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 6, (tblUserGroupCompany x) => x.UserGroupTypeId, (!model.sSortDir_0.Equals("asc")) ? true : false);
			Page<tblUserGroupCompany> result = new Page<tblUserGroupCompany>();
			result = ((UserGroupId != 2 || AccountTypeId != 17) ? objUser.GetGroups(model.iDisplayLength, PageNo, filters, sorts) : objUser.GetSupplierAdminGroups(model.iDisplayLength, PageNo, filters, sorts, UserId));
			List<tblUserGroupCompany> lst = result.Results.ToList();
			List<vm_GroupList> output = Mapper.Map<List<tblUserGroupCompany>, List<vm_GroupList>>(lst);
			foreach (vm_GroupList item in output)
			{
				item.CompanyName = (IsEnglish ? item.CompanyNameEN : item.CompanyNameAR);
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

	public ActionResult AddGroup()
	{
		base.ViewBag.Title = Translation.AddGroup;
		base.ViewBag.SubTitle = Translation.AddGroupDesc;
		return View();
	}

	public async Task<ActionResult> EditGroup(string Id)
	{
		try
		{
			base.ViewBag.Title = Translation.EditGroup;
			base.ViewBag.SubTitle = Translation.AddGroupDesc;
			int GroupId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
			vm_GroupCompanies model = await objUser.GetGroupById(GroupId);
			base.ViewBag.Logo = model.CompanyLogo;
			model.CompanyLogo = "";
			return View("AddGroup", model);
		}
		catch (Exception)
		{
		}
		return HttpNotFound();
	}

	[HttpPost]
	public async Task<JsonResult> AddEditGroup(vm_GroupCompanies model)
	{
		vm_jsOutput output = new vm_jsOutput();
		try
		{
			string strImage = model.CompanyLogo;
			string strExt = "";
			if (!string.IsNullOrEmpty(strImage))
			{
				strExt = (model.CompanyLogo = Path.GetExtension(model.CompanyLogo));
			}
			if (model.UserGroupId > 0)
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objUser.EditGroup(model);
				if (output.StatusId > 0)
				{
					output.Message = Translation.success_UpdateInfo;
				}
			}
			else
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objUser.AddGroup(model);
				if (output.StatusId > 0)
				{
					output.Message = Translation.success_AddGroup;
				}
			}
			if (output.StatusId > 0 && !string.IsNullOrEmpty(strImage))
			{
				string Id = output.StatusId.ToString();
				string strPath = cls_Defaults.UploadPath + cls_Defaults.CompanyLogo;
				if (!Directory.Exists(strPath))
				{
					Directory.CreateDirectory(strPath);
				}
				if (System.IO.File.Exists(strPath + Id + strExt))
				{
					System.IO.File.Delete(strPath + Id + strExt);
				}
				System.IO.File.Move(strPath + strImage, strPath + Id + strExt);
			}
		}
		catch (Exception)
		{
		}
		return Json(output);
	}

	public JsonResult GroupEmailExist(string Email, int? UserGroupId)
	{
		bool ifEmailExist = false;
		try
		{
			db_User objUser = new db_User();
			ifEmailExist = objUser.GroupEmailExists(Email, UserGroupId);
			return Json(!ifEmailExist, JsonRequestBehavior.AllowGet);
		}
		catch (Exception)
		{
			return Json(false, JsonRequestBehavior.AllowGet);
		}
	}

	public JsonResult BindUserGroupSuper(int Id)
	{
		string groupid = base.Session[cls_Defaults.Session_UserGroupId].ToString();
		List<SelectListItem> model = objUser.GetGroupByType(Id, Convert.ToInt32(groupid));
		return Json(model, JsonRequestBehavior.AllowGet);
	}

	public JsonResult BindUserGroup(int Id)
	{
		string groupid = "0";
		string userId = "0";
		int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
		int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
		if (base.Session[cls_Defaults.Session_UserGroupId] != null && base.Session[cls_Defaults.Session_UserId] != null)
		{
			groupid = base.Session[cls_Defaults.Session_UserGroupId].ToString();
			userId = base.Session[cls_Defaults.Session_UserId].ToString();
		}
		List<SelectListItem> model = new List<SelectListItem>();
		model = ((userGroupTypeId != 2 || accountTypeId != 17) ? objUser.GetGroupByType(Id, Convert.ToInt32(groupid)) : objUser.GetSupplierGroupByType(Id, Convert.ToInt32(userId)));
		return Json(model, JsonRequestBehavior.AllowGet);
	}

	public JsonResult BindAccountType(int Id)
	{
		int typeid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
		if (typeid == 2 && Id == 10)
		{
			List<SelectListItem> model3 = cls_DropDowns.DDL_SupplierAccountTypes();
			return Json(model3, JsonRequestBehavior.AllowGet);
		}
		if (typeid == 2 && Id == 1)
		{
			List<SelectListItem> model2 = cls_DropDowns.DDL_SupplierSPAccountTypes();
			List<SelectListItem> model4 = objUser.GetAccountTypeSPAdmin(1);
			return Json(model2, JsonRequestBehavior.AllowGet);
		}
		List<SelectListItem> model = objUser.GetAccountType(Id);
		List<SelectListItem> Default = objUser.GetAccountType(0);
		return Json(model, JsonRequestBehavior.AllowGet);
	}

	[HttpPost]
	public JsonResult DelImgGroup(int Id)
	{
		vm_jsOutput output = new vm_jsOutput();
		try
		{
			Tuple<int, string> status = objUser.DeleteGroupImage(Id);
			if (status.Item1 > 0)
			{
				string strPath = cls_Defaults.UploadPath + cls_Defaults.CompanyLogo;
				if (System.IO.File.Exists(strPath + status.Item2))
				{
					System.IO.File.Delete(strPath + status.Item2);
				}
				output.StatusId = status.Item1;
			}
		}
		catch (Exception)
		{
		}
		return Json(output);
	}

	public ActionResult Index()
	{
		return View();
	}

	public async Task<ActionResult> GetUsers(vm_JqueryDataTables model, string Name, string Email, string Telephone, int StatusId, int TypeId)
	{
		vm_Result objResult = new vm_Result();
		try
		{
			int PageNo = 1;
			if (model.iDisplayStart >= model.iDisplayLength)
			{
				PageNo = model.iDisplayStart / model.iDisplayLength + 1;
			}
			int CurrentUserId = (int)base.Session[cls_Defaults.Session_UserId];
			Filters<tblAdminUser> filters = new Filters<tblAdminUser>();
			Sorts<tblAdminUser> sorts = new Sorts<tblAdminUser>();
			filters.Add(!string.IsNullOrEmpty(Name), (tblAdminUser x) => x.FirstName.Contains(Name) || x.LastName.Contains(Name));
			filters.Add(!string.IsNullOrEmpty(Email), (tblAdminUser x) => x.Email.Equals(Email));
			filters.Add(!string.IsNullOrEmpty(Telephone), (tblAdminUser x) => x.MobileNo.Equals(Telephone));
			filters.Add(StatusId > 0, (tblAdminUser x) => x.Status == ((StatusId == 1) ? true : false));
			filters.Add(TypeId > 0, (tblAdminUser x) => (int?)x.UserGroupTypeId == (int?)TypeId);
			filters.Add(CurrentUserId > 0, (tblAdminUser x) => x.AddedBy == CurrentUserId);
			sorts.Add(model.iSortCol_0 == 0, (tblAdminUser x) => x.FirstName, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 1, (tblAdminUser x) => x.Email, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 2, (tblAdminUser x) => x.MobileNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 3, (tblAdminUser x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 4, (tblAdminUser x) => x.UserGroupTypeId, (!model.sSortDir_0.Equals("asc")) ? true : false);
			sorts.Add(model.iSortCol_0 == 5, (tblAdminUser x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
			db_User objUser = new db_User();
			Page<tblAdminUser> result = objUser.GetUsers(model.iDisplayLength, PageNo, filters, sorts);
			List<tblAdminUser> lst = result.Results.ToList();
			List<vm_UserList> output = Mapper.Map<List<tblAdminUser>, List<vm_UserList>>(lst);
			foreach (vm_UserList item in output)
			{
				item.CompanyName = (IsEnglish ? item.CompanyNameEN : item.CompanyNameAR);
				vm_UserList vm_UserList = item;
				vm_UserList.AccountName = await objUser.GetAccountName(item.AccountTypeId);
				if (item.IsLogin && (item.AccountTypeId == 10 || item.AccountTypeId == 11))
				{
					string Status = (item.AccountName = "<p style='color:Green'>" + item.AccountName + "</p>");
				}
				else if (!item.IsLogin && (item.AccountTypeId == 10 || item.AccountTypeId == 11))
				{
					string Status = (item.AccountName = "<p style='color:Red'>" + item.AccountName + "</p>");
				}
				else
				{
					item.AccountName = item.AccountName;
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

	public ActionResult AddUser()
	{
		return View();
	}

	public async Task<ActionResult> EditUser(string Id)
	{
		try
		{
			int UserId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
			vm_User model = await objUser.GetUserById(UserId);
			base.ViewBag.Logo = model.ProfilePic;
			db_Settings objSettings = new db_Settings();
			base.ViewBag.UserGroup = objSettings.SelectUserGroup(Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]));
			List<SelectListItem> AccountType = objUser.GetAccountType(model.UserGroupTypeId);
			if (model.UserGroupTypeId == 6 || model.UserGroupTypeId == 0)
			{
				base.ViewBag.AccountType = AccountType;
			}
			else
			{
				List<SelectListItem> Default = objUser.GetAccountType(0);
				IEnumerable<SelectListItem> listFinal = AccountType.Union(Default);
				base.ViewBag.AccountType = listFinal;
			}
			return View(model);
		}
		catch (Exception)
		{
		}
		return HttpNotFound();
	}

	[HttpPost]
	public async Task<JsonResult> AddEditUser(vm_User model)
	{
		vm_jsOutput output = new vm_jsOutput();
		try
		{
			int typeid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
			if (!model.UserGroupId.HasValue && typeid != 8)
			{
				model.UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
			}
			if (typeid == 2 && model.UserGroupTypeId != 1)
			{
				model.UserGroupTypeId = Convert.ToByte(typeid);
			}
			db_User objUser = new db_User();
			string strImage = model.ProfilePic;
			string strExt = "";
			if (!string.IsNullOrEmpty(strImage))
			{
				strExt = (model.ProfilePic = Path.GetExtension(model.ProfilePic));
			}
			if (model.UserGroupTypeId != 3 && model.UserGroupId == 0)
			{
				output.Message = Translation.ReqUserGroup;
				return Json(output);
			}
			if (model.UserId > 0)
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objUser.EditUser(model);
			}
			else if (model.AccountTypeId == 10)
			{
				db_Settings obj = new db_Settings();
				tblSetting wokrhrs = obj.GetEorkinhHrsysettings(model.UserGroupId);
				if (Convert.ToInt32(wokrhrs.KeyValue) <= 0)
				{
					output.Message = "Please add working Hours first";
					return Json(output);
				}
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objUser.AddUser(model);
				await cls_Sms.NewUser(output.StatusId, model.Email, model.ConfirmPassword);
			}
			else
			{
				vm_jsOutput vm_jsOutput = output;
				vm_jsOutput.StatusId = await objUser.AddUser(model);
				await cls_Sms.NewUser(output.StatusId, model.Email, model.ConfirmPassword);
			}
			if (output.StatusId > 0 && !string.IsNullOrEmpty(strImage))
			{
				string Id = output.StatusId.ToString();
				string strPath = cls_Defaults.UploadPath + cls_Defaults.ProfilePic;
				if (!Directory.Exists(strPath + strImage))
				{
					Directory.CreateDirectory(strPath);
				}
				if (System.IO.File.Exists(strPath + Id + strExt))
				{
					System.IO.File.Delete(strPath + Id + strExt);
				}
				System.IO.File.Move(strPath + strImage, strPath + Id + strExt);
			}
		}
		catch (Exception)
		{
		}
		return Json(output);
	}

	public JsonResult UserEmailExist(string Email, int? UserId)
	{
		bool ifEmailExist = false;
		try
		{
			db_User objUser = new db_User();
			ifEmailExist = objUser.UserEmailExists(Email, UserId);
			return Json(!ifEmailExist, JsonRequestBehavior.AllowGet);
		}
		catch (Exception)
		{
			return Json(false, JsonRequestBehavior.AllowGet);
		}
	}

	public JsonResult UserMobileNoExist(string MobileNo, int? UserId)
	{
		bool ifEmailExist = false;
		try
		{
			db_User objUser = new db_User();
			ifEmailExist = objUser.UserMobileNoExists(MobileNo, UserId);
			return Json(!ifEmailExist, JsonRequestBehavior.AllowGet);
		}
		catch (Exception)
		{
			return Json(false, JsonRequestBehavior.AllowGet);
		}
	}

	public ActionResult UpdatePassword(string Id)
	{
		vm_EditPassword model = new vm_EditPassword
		{
			UserId = Id
		};
		return PartialView("_UpdatePassword", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<JsonResult> UpdatePassword(vm_EditPassword model)
	{
		vm_jsOutput output = new vm_jsOutput();
		try
		{
			int UserId = Convert.ToInt32(EncryptDecrypt.Decrypt(model.UserId));
			vm_jsOutput vm_jsOutput = output;
			vm_jsOutput.StatusId = await objUser.UpdatePassword(UserId, model.Password);
		}
		catch (Exception)
		{
		}
		return Json(output);
	}

	public ActionResult EditProfile()
	{
		return View();
	}

	[HttpPost]
	public JsonResult DelImgUser(int Id)
	{
		vm_jsOutput output = new vm_jsOutput();
		try
		{
			Tuple<int, string> status = objUser.DeleteUserImage(Id);
			if (status.Item1 > 0)
			{
				string strPath = cls_Defaults.UploadPath + cls_Defaults.ProfilePic;
				if (System.IO.File.Exists(strPath + status.Item2))
				{
					System.IO.File.Delete(strPath + status.Item2);
				}
				output.StatusId = status.Item1;
			}
		}
		catch (Exception)
		{
		}
		return Json(output);
	}
}
}
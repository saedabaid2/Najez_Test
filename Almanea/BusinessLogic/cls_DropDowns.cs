using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Almanea.Data;
using Almanea.Models;

namespace Almanea.BusinessLogic
{

	public static class cls_DropDowns
	{
		public static List<SelectListItem> DDL_Meridian()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			model.Add(new SelectListItem
			{
				Value = "1",
				Text = Translation.AM
			});
			model.Add(new SelectListItem
			{
				Value = "2",
				Text = Translation.PM
			});
			return model;
		}

		public static string GetMeridian(byte? Id)
		{
			switch (Id)
			{
				case 1:
					return Translation.AM;
				case 2:
					return Translation.PM;
			}
			return "";
		}
		public static List<SelectListItem> DDL_Hour()
		{
			int calt = 3;
			List<SelectListItem> model = new List<SelectListItem>();
			string s = DateTime.Now.ToString("hh");
			for (int i = 1; i <= 24; i++)
			{
				int gettcstart = Convert.ToInt32(i);
				if (i > 1)
				{
					gettcstart = Convert.ToInt32(i) + Convert.ToInt32(i) * 3;
				}
				int gettc = gettcstart + 3;
				if (gettcstart >= 24)
				{
					break;
				}
				model.Add(new SelectListItem
				{
					Value = i.ToString(),
					Text = gettcstart + "-" + gettc
				});
			}
			return model;
		}

		public static List<SelectListItem> DDL_Hour1()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			string s = DateTime.Now.ToString("hh");
			for (int i = 1; i <= 12; i++)
			{
				model.Add(new SelectListItem
				{
					Value = i.ToString(),
					Text = i.ToString()
				});
			}
			return model;
		}

		public static List<SelectListItem> DDL_Hour(int flag)
		{
			List<SelectListItem> model = new List<SelectListItem>();
			string s = DateTime.Now.ToString("hh");
			if (flag == 1)
			{
				for (int j = 1; j <= 12; j++)
				{
					if (Convert.ToInt32(s) <= j)
					{
						model.Add(new SelectListItem
						{
							Value = j.ToString(),
							Text = j.ToString()
						});
					}
				}
			}
			else
			{
				for (int i = 1; i <= 12; i++)
				{
					model.Add(new SelectListItem
					{
						Value = i.ToString(),
						Text = i.ToString()
					});
				}
			}
			return model;
		}

		public static List<SelectListItem> DDL_UserGroupTypes(int IsAdmin = 0)
		{
			List<SelectListItem> model = new List<SelectListItem>();
			switch (Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]))
			{
				case 3:
					model.Add(new SelectListItem
					{
						Value = "1",
						Text = Translation.ServiceProvider
					});
					model.Add(new SelectListItem
					{
						Value = "7",
						Text = Translation.Executive
					});
					model.Add(new SelectListItem
					{
						Value = "9",
						Text = Translation.SellerStaff
					});
					break;
				case 8:
					model.Add(new SelectListItem
					{
						Value = "1",
						Text = Translation.ServiceProvider
					});
					model.Add(new SelectListItem
					{
						Value = "2",
						Text = Translation.Supplier
					});
					model.Add(new SelectListItem
					{
						Value = "10",
						Text = Translation.User
					});
					break;
				case 2:
					model.Add(new SelectListItem
					{
						Value = "1",
						Text = Translation.ServiceProvider
					});
					model.Add(new SelectListItem
					{
						Value = "10",
						Text = Translation.User
					});
					break;
			}
			return model;
		}

		public static List<SelectListItem> DDL_AccountTypes(int IsAdmin = 0)
		{
			List<SelectListItem> model = new List<SelectListItem>();
			int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
			if (IsAdmin == 1)
			{
				model.Add(new SelectListItem
				{
					Value = "0",
					Text = Translation.Admin
				});
				model.Add(new SelectListItem
				{
					Value = "12",
					Text = Translation.SellerSupervisor
				});
				model.Add(new SelectListItem
				{
					Value = "13",
					Text = Translation.SellerAgent
				});
			}
			else
			{
				model.Add(new SelectListItem
				{
					Value = "0",
					Text = Translation.Admin
				});
			}
			return model;
		}

		public static List<SelectListItem> DDL_SupplierSPAccountTypes()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(20).ToString(),
				Text = Translation.Admin
			});
			return model;
		}

		public static List<SelectListItem> DDL_UserAccountTypes()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			model.Add(new SelectListItem
			{
				Value = "19",
				Text = Translation.Admin
			});
			return model;
		}

		public static List<SelectListItem> DDL_SupplierAccountTypes()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(2).ToString(),
				Text = Translation.CallCenter
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(3).ToString(),
				Text = Translation.Finance
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(14).ToString(),
				Text = Translation.SellerSupervisor
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(15).ToString(),
				Text = Translation.SellerAgent
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(17).ToString(),
				Text = Translation.Admin
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(18).ToString(),
				Text = Translation.Executive
			});
			return model;
		}

		public static List<SelectListItem> DDL_ServiceProviderAccountTypes()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(6).ToString(),
				Text = Translation.Supervisor
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(7).ToString(),
				Text = Translation.Agent
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(10).ToString(),
				Text = Translation.Labour
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(11).ToString(),
				Text = Translation.Driver
			});
			model.Add(new SelectListItem
			{
				Value = Convert.ToInt32(20).ToString(),
				Text = Translation.Admin
			});
			return model;
		}

		public static List<SelectListItem> GetLocations(int UserGroupId, int UserGroupTypeId, int AccountTypeId)
		{
			db_Settings objSetting = new db_Settings();
			List<Category> categories = new List<Category>();
			if ((UserGroupTypeId == 2 && AccountTypeId == 17) || AccountTypeId == 18 || AccountTypeId == 15 || AccountTypeId == 14)
			{
				return objSetting.GetSupplierAdminLocations(UserGroupId.ToString());
			}
			if (UserGroupTypeId == 1)
			{
				return objSetting.GetServiceProviderLocations();
			}
			return objSetting.GetLocations();
		}

		public static List<SelectListItem> GetSupplierAdminAgent(int UserGroupId, int UserGroupTypeId, int AccountTypeId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetSupplierAdminAgent(UserGroupId);
		}

		public static List<SelectListItem> GetSupplierAdminLocations()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetLocations();
		}

		public static List<SelectListItem> GetServicesList()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetServicesList();
		}

		public static List<SelectListItem> GetSupplierAdminServicesList(int UserGroupId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetSupplierAdminServicesList(UserGroupId);
		}

		public static List<vm_AdditionalWork> GetAdditionalWorksCat(int cat)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.DDLAdditionlWork2(cat);
		}

		public static List<SelectListItem> GetService()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetServices();
		}

		public static List<SelectListItem> GetService(int cat)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetServices(cat);
		}

		public static List<SelectListItem> GetSallerAgentService()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetServices();
		}

		public static List<SelectListItem> GetCategory()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.DDLComplainType();
		}

		public static List<SelectListItem> GetSupplierCategoryList()
		{
			db_Settings objSetting = new db_Settings();
			int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
			int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
			int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
			int ServiceProviderId = objSetting.GetSupplierOrProviderAdminId(UserId);
			tblAdminUser SupplierAdmin = objSetting.GetSupplierAdmin(ServiceProviderId);
			return objSetting.DDCategory(SupplierAdmin.UserGroupId.Value);
		}

		public static List<SelectListItem> GetCategoryList()
		{
			db_Settings objSetting = new db_Settings();
			int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
			return objSetting.DDCategory(UserGroupTypeId);
		}


		public static string OrderStatusName(int Id)
		{
			if (Id == (int)OrderStatus.NewOrder)
				return Translation.New;
			else if (Id == (int)OrderStatus.Reserved)
				return Translation.Reserved;
			else if (Id == (int)OrderStatus.AssignDriver)
				return Translation.AssignDriver;
			else if (Id == (int)OrderStatus.AssignLabour)
				return Translation.AssignLabour;
			else if (Id == (int)OrderStatus.AppointmentConfirmed)
				return Translation.AppointmentConfirmed;
			else if (Id == (int)OrderStatus.PartialDelivery)
				return Translation.PartialDelivery;
			else if (Id == (int)OrderStatus.ReceivedFromWarehouse)
				return Translation.ReceivedFromWarehouse;
			else if (Id == (int)OrderStatus.AppointmentReschedule)
				return Translation.AppointmentReschedule;
			else if (Id == (int)OrderStatus.Job_in_Progress)
				return Translation.Job_in_Progress;
			else if (Id == (int)OrderStatus.ChangeService)
				return Translation.ChangeService;
			else if (Id == (int)OrderStatus.Finish)
				return Translation.FinishWork;
			else if (Id == (int)OrderStatus.Complete)
				return Translation.Complete;
			else if (Id == (int)OrderStatus.Delete)
				return Translation.Delete;
			else if (Id == (int)OrderStatus.Cancel)
				return Translation.Cancel;
			else if (Id == (int)OrderStatus.ReSchedule)
				return Translation.ReSchedule;
			else if (Id == (int)OrderStatus.Rejected)
				return Translation.Rejected;
			else if (Id == (int)OrderStatus.HoldOn)
				return Translation.HoldOn;
			else if (Id == (int)OrderStatus.Postponed)
				return Translation.Postpone;
			return "";
		}
		public static List<SelectListItem> AllOrderStatus()
		{
			List<SelectListItem> model = new List<SelectListItem>();
			model.Add(new SelectListItem
			{
				Value = 1.ToString(),
				Text = Translation.NewOrder
			});
			model.Add(new SelectListItem
			{
				Value = 2.ToString(),
				Text = Translation.Reserved
			});
			model.Add(new SelectListItem
			{
				Value = 17.ToString(),
				Text = Translation.AssignDriver
			});
			model.Add(new SelectListItem
			{
				Value = 18.ToString(),
				Text = Translation.AssignLabour
			});
			model.Add(new SelectListItem
			{
				Value = 5.ToString(),
				Text = Translation.AppointmentConfirmed
			});
			model.Add(new SelectListItem
			{
				Value = 16.ToString(),
				Text = Translation.PartialDelivery
			});
			model.Add(new SelectListItem
			{
				Value = 14.ToString(),
				Text = Translation.ReceivedFromWarehouse
			});
			model.Add(new SelectListItem
			{
				Value = 15.ToString(),
				Text = Translation.AppointmentReschedule
			});
			model.Add(new SelectListItem
			{
				Value = 6.ToString(),
				Text = Translation.Job_in_Progress
			});
			model.Add(new SelectListItem
			{
				Value = 7.ToString(),
				Text = Translation.HoldOn
			});
			model.Add(new SelectListItem
			{
				Value = 8.ToString(),
				Text = Translation.ChangeService
			});
			model.Add(new SelectListItem
			{
				Value = 9.ToString(),
				Text = Translation.FinishWork
			});
			model.Add(new SelectListItem
			{
				Value = 10.ToString(),
				Text = Translation.Complete
			});
			return model;
		}

		public static List<SelectListItem> Get_OrderStatus(int currentStatus)
		{
			List<SelectListItem> model = new List<SelectListItem>();
			int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
			int ActtypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_AccountTypeId]);
			switch (currentStatus)
			{
				case 1:
					model.Add(new SelectListItem
					{
						Value = 2.ToString(),
						Text = Translation.Reserved
					});
					break;
				case 9:
					model.Add(new SelectListItem
					{
						Value = 10.ToString(),
						Text = Translation.Complete
					});
					break;
				default:
					model.Add(new SelectListItem
					{
						Value = 4.ToString(),
						Text = "3-" + Translation.AppointmentConfirmed
					});
					model.Add(new SelectListItem
					{
						Value = 16.ToString(),
						Text = "4-" + Translation.PartialDelivery
					});
					model.Add(new SelectListItem
					{
						Value = 14.ToString(),
						Text = "5-" + Translation.ReceivedFromWarehouse
					});
					model.Add(new SelectListItem
					{
						Value = 15.ToString(),
						Text = "6-" + Translation.AppointmentReschedule
					});
					model.Add(new SelectListItem
					{
						Value = 6.ToString(),
						Text = "7-" + Translation.Job_in_Progress
					});
					model.Add(new SelectListItem
					{
						Value = 7.ToString(),
						Text = "8-" + Translation.HoldOn
					});
					model.Add(new SelectListItem
					{
						Value = 8.ToString(),
						Text = "9-" + Translation.ChangeService
					});
					model.Add(new SelectListItem
					{
						Value = 9.ToString(),
						Text = "10-" + Translation.FinishWork
					});
					model.Add(new SelectListItem
					{
						Value = 20.ToString(),
						Text = "20-" + Translation.Postpone
					});
					if (UserGroupTypeId == 3 || UserGroupTypeId == 8 || ActtypeId == 17)
					{
						model = new List<SelectListItem>();
						model.Add(new SelectListItem
						{
							Value = 10.ToString(),
							Text = "11-" + Translation.Complete
						});
						model.Add(new SelectListItem
						{
							Value = 12.ToString(),
							Text = "12-" + Translation.Cancel
						});
					}
					else if (UserGroupTypeId == 1 || ActtypeId == 20)
					{
						model.Add(new SelectListItem
						{
							Value = 10.ToString(),
							Text = "9-" + Translation.Complete
						});
						if (ActtypeId != 7)
						{
							model.Add(new SelectListItem
							{
								Value = 12.ToString(),
								Text = "10-" + Translation.Cancel
							});
						}
					}
					else if (UserGroupTypeId == 1 || ActtypeId == 20)
					{
						model.Add(new SelectListItem
						{
							Value = 10.ToString(),
							Text = "9-" + Translation.Complete
						});
						model.Add(new SelectListItem
						{
							Value = 12.ToString(),
							Text = "10-" + Translation.Cancel
						});
					}
					break;
			}
			return model;
		}

		public static List<SelectListItem> Admin_Get_OrderStatus(int currentStatus)
		{
			List<SelectListItem> model = new List<SelectListItem>();
			int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
			if (currentStatus == 1)
			{
				model.Add(new SelectListItem
				{
					Value = 2.ToString(),
					Text = Translation.Reserved
				});
				model.Add(new SelectListItem
				{
					Value = 3.ToString(),
					Text = Translation.Rejected
				});
			}
			else
			{
				model.Add(new SelectListItem
				{
					Value = 4.ToString(),
					Text = "1-" + Translation.AppointmentConfirmed
				});
				model.Add(new SelectListItem
				{
					Value = 16.ToString(),
					Text = "2-" + Translation.PartialDelivery
				});
				model.Add(new SelectListItem
				{
					Value = 14.ToString(),
					Text = "3-" + Translation.ReceivedFromWarehouse
				});
				model.Add(new SelectListItem
				{
					Value = 15.ToString(),
					Text = "4-" + Translation.AppointmentReschedule
				});
				model.Add(new SelectListItem
				{
					Value = 6.ToString(),
					Text = "5-" + Translation.Job_in_Progress
				});
				model.Add(new SelectListItem
				{
					Value = 7.ToString(),
					Text = "6-" + Translation.HoldOn
				});
				model.Add(new SelectListItem
				{
					Value = 8.ToString(),
					Text = "7-" + Translation.ChangeService
				});
				model.Add(new SelectListItem
				{
					Value = 9.ToString(),
					Text = "8-" + Translation.FinishWork
				});
				if (UserGroupTypeId == 3 || UserGroupTypeId == 8)
				{
					model.Add(new SelectListItem
					{
						Value = 10.ToString(),
						Text = "9-" + Translation.Complete
					});
					model.Add(new SelectListItem
					{
						Value = 12.ToString(),
						Text = "10-" + Translation.Cancel
					});
					model.Add(new SelectListItem
					{
						Value = 13.ToString(),
						Text = "11-" + Translation.Release
					});
				}
			}
			return model;
		}

		public static string UserGroupName(int Id)
		{
			switch (Id)
			{
				case 1:
					return "Provider";
				case 2:
					return "Supplier";
				case 3:
					return "Admin";
				case 7:
					return "Executive";
				case 8:
					return "SuperAdmin";
				case 9:
					return "SellerStaff";
				case 10:
					return "User";

					//case 4: return "Warehouse";
			}
			return "";
		}

		public static string GetBlockDate(int? Id)
		{
			db_Settings obj = new db_Settings();
			string user = obj.GetLaborBlockDate(Convert.ToInt32(Id));
			if (user == null)
			{
				return "";
			}
			if (user != null)
			{
				return user;
			}
			return "";
		}

		public static string GetLaborDriverName(int? Id, int? iddriver)
		{
			string text = ((Id > 0) ? "Assigned Labour" : "Assigned Driver");
			int? userodd = ((Id > 0) ? Id : iddriver);
			db_User obj = new db_User();
			tblAdminUser user = obj.GetUser(Convert.ToInt32(userodd));
			if (user == null)
			{
				return "";
			}
			text = ((user == null) ? "" : text);
			return text + "  :  " + ((user == null) ? "" : (user.FirstName + "  " + user.LastName));
		}

		public static string GetGroupName(byte? Id)
		{
			switch (Id)
			{
				case 1:
					return Translation.ServiceProvider;
				case 2:
					return Translation.Supplier;
				case 3:
					return Translation.Admin;
				case 4:
					return Translation.Labour;
				case 5:
					return Translation.Driver;
				case 7:
					return Translation.Executive;
				case 8:
					return Translation.SuperAdmin;
				case 9:
					return Translation.SellerStaff;
				case 10:
					return Translation.User;
			}
			return "";
		}

		public static string ComplainStatus(byte? Id)
		{
			if (Id == 1)
			{
				return Translation.New;
			}
			if (Id == 2)
			{
				return Translation.AssignSp;
			}
			if (Id == 3)
			{
				return Translation.Resolve;
			}
			if (Id == 4)
			{
				return Translation.Reject;
			}
			if (Id == 10)
			{
				return Translation.ResolvedbyAgent;
			}
			if (Id == 9)
			{
				return Translation.VerifyingbyAgent;
			}
			if (HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId] != null)
			{
				int userGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
				if (userGroupTypeId == 9)
				{
					return Translation.AssignSp;
				}
				if (Id == 7)
				{
					return Translation.RejectBySp;
				}
				return Translation.ResolveBySp;
			}
			return Translation.AssignSp;
		}

		public static string ComplainBy(byte? Id)
		{
			if (Id == 1)
			{
				return Translation.Customer;
			}
			if (Id == 2)
			{
				return Translation.Supplier;
			}
			if (Id == 2)
			{
				return Translation.Admin;
			}
			return "";
		}

		public static List<SelectListItem> GetCompanies()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.DDLCompanies();
		}

		public static List<SelectListItem> GetCompanies(int addedBy)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.DDLCompanies(addedBy);
		}

		public static List<SelectListItem> GetSupplierandSupervisorCompanies(int UserId, int UserGroupId, int UserGroupTypeId, int AccountTypeId)
		{
			db_Settings objSetting = new db_Settings();
			if (UserGroupTypeId == 2 && AccountTypeId == 17)
			{
				return objSetting.DDLCompanies(UserId);
			}
			if (UserGroupTypeId == 2 && AccountTypeId == 14)
			{
				int SupplierId = objSetting.GetSupplierOrProviderAdminId(UserId);
				return objSetting.DDLCompanies(SupplierId);
			}
			return objSetting.DDLCompanies();
		}

		public static List<SelectListItem> GetSupplierCompanies(string userId, string userGroupId, string userGroupTypeId, string accountTypeId)
		{
			db_Settings objSetting = new db_Settings();
			int UserId = Convert.ToInt32(userId);
			int UserGroupId = Convert.ToInt32(userGroupId);
			int UserGroupTypeId = Convert.ToInt32(userGroupTypeId);
			int AccountTypeId = Convert.ToInt32(accountTypeId);
			return objSetting.DDLCompanies();
		}

		public static List<SelectListItem> GetAjentUsers()
		{
			int userGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
			int id = 2;
			db_Settings objSetting = new db_Settings();
			return objSetting.DDLAjentUsers(userGroupTypeId);
		}

		public static List<SelectListItem> GetAjentUsersById(int userGroupTypeId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.DDLAjentUsers(userGroupTypeId);
		}

		public static List<SelectListItem> GetProviderList(int userGroupTypeId, int accountTypeId, int userId)
		{
			db_Settings objSetting = new db_Settings();
			if (userGroupTypeId == 2 && accountTypeId == 17)
			{
				return objSetting.GetSupplierProviderList(userId);
			}
			return objSetting.GetProviderList();
		}

		public static List<SelectListItem> GetCategoryList(int userGroupId, int userGroupTypeId, int accountTypeId)
		{
			db_Settings objSetting = new db_Settings();
			List<Category> categories = new List<Category>();
			if (userGroupTypeId == 2 && accountTypeId == 17)
			{
				return objSetting.GetSupplierAdminCategoryList(userGroupId);
			}
			return objSetting.GetCategoryList();
		}

		public static List<SelectListItem> GetOrderComplainCategoryList(int userGroupId, int userGroupTypeId, int accountTypeId, List<int?> OrderServiceCategory)
		{
			db_Settings objSetting = new db_Settings();
			List<Category> categories = new List<Category>();
			if (userGroupTypeId == 2 && accountTypeId == 17)
			{
				return objSetting.GetOrderServiceCategoryList(userGroupId, OrderServiceCategory);
			}
			return objSetting.GetCategoryList();
		}

		public static string GetCategoryList(int categoryid)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetCategoryList(categoryid);
		}

		public static string GetEstimated(int serviceid)
		{
			string GroupTyid = HttpContext.Current.Session[cls_Defaults.Session_UserGroupId].ToString();
			db_Settings objSetting = new db_Settings();
			return objSetting.GetEstimatedtime(serviceid, Convert.ToInt32(GroupTyid));
		}

		public static bool GetIsworking(int serviceid)
		{
			string GroupTyid = HttpContext.Current.Session[cls_Defaults.Session_UserGroupId].ToString();
			db_Settings objSetting = new db_Settings();
			return objSetting.GetIsworking(serviceid, Convert.ToInt32(GroupTyid));
		}

		public static List<SelectListItem> GetProviderList(int serviceId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetProviderList(serviceId);
		}

		public static List<SelectListItem> GetSupplierList()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetSupplierList();
		}

		public static List<SelectListItem> GetSupplier(int UserId)
		{
			db_Settings objSetting = new db_Settings();
			int SupplierId = objSetting.GetSupplierOrProviderAdminId(UserId);
			tblAdminUser Supplier = objSetting.GetSupplierAdmin(SupplierId);
			return objSetting.GetSupplier(Convert.ToInt32(Supplier.UserGroupId));
		}

		public static List<SelectListItem> GetSupplierByuserGroupId(int userGroupId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetSupplier(userGroupId);
		}

		public static List<SelectListItem> GetSupplierAdminSupplierList(string userId, string userGroupId, string userGroupTypeId, string accountTypeId)
		{
			db_Settings objSetting = new db_Settings();
			int UserId = Convert.ToInt32(userId);
			int UserGroupId = Convert.ToInt32(userGroupId);
			int UserGroupTypeId = Convert.ToInt32(userGroupTypeId);
			int AccountTypeId = Convert.ToInt32(accountTypeId);
			return objSetting.GetSupplierList();
		}

		public static List<SelectListItem> GetSupplierList(int serviceId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetSupplierList(serviceId);
		}

		public static string GetProviderOrSupplierByServiceId(int serviceId, int userGroupTypeId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetProviderOrSupplierByServiceId(serviceId, userGroupTypeId);
		}

		public static tblUserGroupCompany GetAllProvider(int id)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetAllProvider(id);
		}

		public static string GetReservedBy(int UserId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetReservedBy(UserId);
		}

		public static tblUserGroupCompany GetAllSupplier(int id)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetAllSupplier(id);
		}

		public static List<vm_Direction> GetDirection()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetDirection();
		}

		public static vm_Direction GetDirection(int Id)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetDirection(Id);
		}

		public static vm_Locations GetLocation(int LocationId)
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetLocation(LocationId);
		}

		public static List<tblUnit> GetUnit()
		{
			db_Settings objSetting = new db_Settings();
			return objSetting.GetUnit();
		}
	}
}
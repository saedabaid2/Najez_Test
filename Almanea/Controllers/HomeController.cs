using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;

namespace Almanea.Controllers
{
    public class HomeController : BaseController
    {
        public class PublicHoliday
        {
            public int Sr { get; set; }

            public string Title { get; set; }

            public string Desc { get; set; }

            public string Start_Date { get; set; }

            public string End_Date { get; set; }
        }

        private db_User objUser = new db_User();

        private db_Settings objSettings = new db_Settings();

        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        public ActionResult Index()
        {
            string password = objSettings.EncryptString("almaneacc", useHashing: false);
            if (base.Session[cls_Defaults.Session_UserId] != null)
            {
                base.Session.Clear();
            }
            return View();
        }

        [SiteAuthorize(new string[] { "Admin", "SuperAdmin", "Supplier" })]
        public ActionResult CalendarSupplier()
        {
            FilterDropDown model = new FilterDropDown();
            List<int?> slectedSPorderlistid = new List<int?>();
            List<tblOrder> order = new List<tblOrder>();
            try
            {
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                if (UserGroupTypeId == 2 && AccountTypeId == 17)
                {
                    List<int> GroupUsers2 = (from u in db.tblAdminUsers
                                             where u.UserGroupId == (int?)UserGroupId
                                             select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers2.Contains(x.AddedBy)).ToList();
                }
                else if (UserGroupTypeId == 1 && AccountTypeId == 20)
                {
                    order = objSettings.GetCalenderOrdersOnlySP(ProviderId);
                }
                else if (UserGroupTypeId == 1 && AccountTypeId == 6)
                {
                    int ServiceProviderAdmin = objSettings.GetSupplierOrProviderAdminId(UserId);
                    int SupllierAdmin = objSettings.GetSupplierOrProviderAdminId(ServiceProviderAdmin);
                    List<int> GroupUsers = (from u in db.tblAdminUsers
                                            where u.AddedBy == SupllierAdmin
                                            select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers.Contains(x.AddedBy) && x.ReservedProvider == ProviderId).ToList();
                }
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(order);
                List<string> DateList = new List<string>();
                DateList = (from x in order
                            group x by x.InstallDate into x
                            select x.Key.ToString()).ToList();
                List<VmOrderCalendar> calenderModelList = new List<VmOrderCalendar>();
                VmOrderCalendar calendarModel = new VmOrderCalendar();
                int accountType = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                string baseAddress = string.Empty;
                baseAddress = ((AccountTypeId == 6 && UserGroupTypeId == 1) ? "/Provider/NewOrders" : ((AccountTypeId != 20 || UserGroupTypeId != 1) ? "/Setting/Orders" : "/Provider/NewOrders"));
                calendarModel = CalendarData(model, output, DateList, calenderModelList, calendarModel, baseAddress);
                base.ViewBag.Events = calenderModelList;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return View(model);
        }

        [SiteAuthorize(new string[] { "Admin", "SuperAdmin", "Supplier" })]
        [HttpPost]
        public ActionResult CalendarSupplier(FilterDropDown model)
        {
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                List<tblOrder> order = new List<tblOrder>();
                List<int?> slectedSPorderlistid = new List<int?>();
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                if (UserGroupTypeId == 2 && AccountTypeId == 17)
                {
                    List<int> GroupUsers2 = (from u in db.tblAdminUsers
                                             where u.UserGroupId == (int?)UserGroupId
                                             select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers2.Contains(x.AddedBy)).ToList();
                }
                if (UserGroupTypeId == 1 && AccountTypeId == 20)
                {
                    order = objSettings.GetCalenderOrdersOnlySP(ProviderId);
                }
                else if (UserGroupTypeId == 2 && AccountTypeId == 18)
                {
                    int AdminId = objSettings.GetSupplierOrProviderAdminId(UserId);
                    List<int> GroupUsers = (from u in db.tblAdminUsers
                                            where u.AddedBy == AdminId
                                            select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers.Contains(x.AddedBy)).ToList();
                }
                List<tblOrder> orderlist = order;
                if (model.StatusId > 0)
                {
                    orderlist = orderlist.Where((tblOrder x) => x.Status == model.StatusId).ToList();
                }
                if (model.LocationId > 0)
                {
                    orderlist = orderlist.Where((tblOrder x) => x.LocationId == model.LocationId).ToList();
                }
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(orderlist);
                List<string> DateList = new List<string>();
                DateList = (from x in orderlist
                            group x by x.InstallDate into x
                            select x.Key.ToString()).ToList();
                List<VmOrderCalendar> calenderModelList = new List<VmOrderCalendar>();
                VmOrderCalendar calendarModel = new VmOrderCalendar();
                string baseAddress = string.Empty;
                baseAddress = ((AccountTypeId == 6 && UserGroupTypeId == 1) ? "/Provider/NewOrders" : ((AccountTypeId != 20 || UserGroupTypeId != 1) ? "/Setting/Orders" : "/Provider/NewOrders"));
                calendarModel = CalendarData(model, output, DateList, calenderModelList, calendarModel, baseAddress);
                base.ViewBag.Events = calenderModelList;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return View(model);
        }

        [SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
        public ActionResult Calendar()
        {
            FilterDropDown model = new FilterDropDown();
            List<int?> slectedSPorderlistid = new List<int?>();
            List<tblOrder> order = new List<tblOrder>();
            try
            {
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                if (UserGroupTypeId == 2 && AccountTypeId == 18)
                {
                    int AdminId = objSettings.GetSupplierOrProviderAdminId(UserId);
                    List<int> GroupUsers2 = (from u in db.tblAdminUsers
                                             where u.AddedBy == AdminId
                                             select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers2.Contains(x.AddedBy)).ToList();
                }
                else if (UserGroupTypeId == 1 && AccountTypeId == 20)
                {
                    order = objSettings.GetCalenderOrdersOnlySP(ProviderId);
                }
                else if (UserGroupTypeId == 1 && AccountTypeId == 6)
                {
                    int ServiceProviderAdmin = objSettings.GetSupplierOrProviderAdminId(UserId);
                    int SupllierAdmin = objSettings.GetSupplierOrProviderAdminId(ServiceProviderAdmin);
                    List<int> GroupUsers = (from u in db.tblAdminUsers
                                            where u.AddedBy == SupllierAdmin
                                            select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers.Contains(x.AddedBy) && x.ReservedProvider == ProviderId).ToList();
                }
                else
                {
                    List<tblAdminUser> userData = db.tblAdminUsers.ToList();
                    IEnumerable<int> getagentspovider = from x in db.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)ProviderId).ToList()
                                                        select x.UserId;
                    List<OrderDisplay> orderDisplays = db.OrderDisplays.ToList();
                    slectedSPorderlistid = (from x in orderDisplays.Where((OrderDisplay x) => !x.ReservedBy.HasValue || getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                            select x.OrderId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => slectedSPorderlistid.Contains(x.OrderId)).ToList();
                }
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(order);
                List<string> DateList = new List<string>();
                DateList = (from x in order
                            group x by x.InstallDate into x
                            select x.Key.ToString()).ToList();
                List<VmOrderCalendar> calenderModelList = new List<VmOrderCalendar>();
                VmOrderCalendar calendarModel = new VmOrderCalendar();
                int accountType = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                string baseAddress = string.Empty;
                baseAddress = ((AccountTypeId == 6 && UserGroupTypeId == 1) ? "/Provider/NewOrders" : ((AccountTypeId != 20 || UserGroupTypeId != 1) ? "/Setting/Orders" : "/Provider/NewOrders"));
                calendarModel = CalendarData(model, output, DateList, calenderModelList, calendarModel, baseAddress);
                base.ViewBag.Events = calenderModelList;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return View(model);
        }

        [SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
        [HttpPost]
        public ActionResult Calendar(FilterDropDown model)
        {
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                List<tblOrder> order = new List<tblOrder>();
                List<int?> slectedSPorderlistid = new List<int?>();
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                if (UserGroupTypeId == 1 && AccountTypeId == 20)
                {
                    order = objSettings.GetCalenderOrdersOnlySP(ProviderId);
                }
                else if (UserGroupTypeId == 2 && AccountTypeId == 18)
                {
                    int AdminId = objSettings.GetSupplierOrProviderAdminId(UserId);
                    List<int> GroupUsers = (from u in db.tblAdminUsers
                                            where u.AddedBy == AdminId
                                            select u.UserId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => GroupUsers.Contains(x.AddedBy)).ToList();
                }
                else
                {
                    List<tblAdminUser> userData = db.tblAdminUsers.ToList();
                    IEnumerable<int> getagentspovider = from x in db.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)ProviderId).ToList()
                                                        select x.UserId;
                    List<OrderDisplay> orderDisplays = db.OrderDisplays.ToList();
                    slectedSPorderlistid = (from x in orderDisplays.Where((OrderDisplay x) => !x.ReservedBy.HasValue || getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                            select x.OrderId).ToList();
                    order = db.tblOrders.Where((tblOrder x) => slectedSPorderlistid.Contains(x.OrderId)).ToList();
                }
                List<tblOrder> orderlist = order;
                if (model.StatusId > 0)
                {
                    orderlist = orderlist.Where((tblOrder x) => x.Status == model.StatusId).ToList();
                }
                if (model.LocationId > 0)
                {
                    orderlist = orderlist.Where((tblOrder x) => x.LocationId == model.LocationId).ToList();
                }
                if (model.StatusId == 0 && model.LocationId == 0 && model.SupplierId == 0)
                {
                    orderlist = db.tblOrders.OrderByDescending((tblOrder x) => x.OrderId).ToList();
                }
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(orderlist);
                List<string> DateList = new List<string>();
                DateList = (from x in orderlist
                            group x by x.InstallDate into x
                            select x.Key.ToString()).ToList();
                List<VmOrderCalendar> calenderModelList = new List<VmOrderCalendar>();
                VmOrderCalendar calendarModel = new VmOrderCalendar();
                string baseAddress = string.Empty;
                baseAddress = ((AccountTypeId == 6 && UserGroupTypeId == 1) ? "/Provider/NewOrders" : ((AccountTypeId != 20 || UserGroupTypeId != 1) ? "/Setting/Orders" : "/Provider/NewOrders"));
                calendarModel = CalendarData(model, output, DateList, calenderModelList, calendarModel, baseAddress);
                base.ViewBag.Events = calenderModelList;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return View(model);
        }

        private static VmOrderCalendar CalendarData(FilterDropDown model, List<vm_OrderList> output, List<string> DateList, List<VmOrderCalendar> calenderModelList, VmOrderCalendar calenderModel, string baseAddress)
        {
            foreach (string item in DateList)
            {
                if (item != "")
                {
                    List<VmOrderCalendar> ModelList = new List<VmOrderCalendar>();
                    List<vm_OrderList> orderData = output.Where((vm_OrderList x) => x.InstallDate.Substring(0, 10) == item.Substring(0, 10)).ToList();
                    int newOrderCount = orderData.Where((vm_OrderList x) => x.Status == 1).Count();
                    int cancelOrderCount = orderData.Where((vm_OrderList x) => x.Status == 12).Count();
                    int completeOrderCount = orderData.Where((vm_OrderList x) => x.Status == 10).Count();
                    int resrvedOrderCount = orderData.Where((vm_OrderList x) => x.Status == 2).Count();
                    int rejectedOrderCount = orderData.Where((vm_OrderList x) => x.Status == 3).Count();
                    int assignDriverOrderCount = orderData.Where((vm_OrderList x) => x.Status == 17).Count();
                    int assignLabourOrderCount = orderData.Where((vm_OrderList x) => x.Status == 18).Count();
                    int partialDeliveryOrderCount = orderData.Where((vm_OrderList x) => x.Status == 16).Count();
                    int receivedFromwarehouserOrderCount = orderData.Where((vm_OrderList x) => x.Status == 14).Count();
                    int startWorkOrderCount = orderData.Where((vm_OrderList x) => x.Status == 6).Count();
                    int changeServiceOrderCount = orderData.Where((vm_OrderList x) => x.Status == 8).Count();
                    int appointmentconfirmOrderCount = orderData.Where((vm_OrderList x) => x.Status == 5).Count();
                    int finishWorkOrderCount = orderData.Where((vm_OrderList x) => x.Status == 9).Count();
                    int onHoldOrderCount = orderData.Where((vm_OrderList x) => x.Status == 7).Count();
                    int appointmentRescheduleOrderCount = orderData.Where((vm_OrderList x) => x.Status == 15).Count();
                    if (newOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment newOrderColor";
                        calenderModel.title = Translation.New_Order + "(" + SetZeroBeforData(newOrderCount) + ")";
                        string ddd13 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 1 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (cancelOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment cancelOrderColor";
                        calenderModel.title = Translation.Cancelled_Order + " (" + SetZeroBeforData(cancelOrderCount) + ")";
                        string ddd14 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = "/Provider/Rejected?StatusId=" + 12 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (completeOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment completeOrderColor";
                        calenderModel.title = Translation.Completed_Orders + " (" + SetZeroBeforData(completeOrderCount) + ")";
                        string ddd15 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = "/Provider/Finished?StatusId=" + 10 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (resrvedOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment resrvedOrderColor";
                        calenderModel.title = Translation.Reserved_Order + " (" + SetZeroBeforData(resrvedOrderCount) + ")";
                        string ddd12 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 2 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (rejectedOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment rejectedOrderColor";
                        calenderModel.title = Translation.Rejected_Order + " (" + SetZeroBeforData(rejectedOrderCount) + ")";
                        string ddd11 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = "/Provider/Rejected?StatusId=" + 3 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (assignDriverOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment assignDriverOrderColor";
                        calenderModel.title = Translation.Order_Assigned_to_Driver + " (" + SetZeroBeforData(assignDriverOrderCount) + ")";
                        string ddd10 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 17 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (assignLabourOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment assignLabourOrderColor";
                        calenderModel.title = Translation.Order_Assigned_to_Labour + " (" + SetZeroBeforData(assignLabourOrderCount) + ")";
                        string ddd9 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 18 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (partialDeliveryOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment partialDeliveryOrderColor";
                        calenderModel.title = Translation.Order_Partial_Delivery + " (" + SetZeroBeforData(partialDeliveryOrderCount) + ")";
                        string ddd8 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 16 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (receivedFromwarehouserOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment receivedFromwarehouserOrderColor";
                        calenderModel.title = Translation.ReceivedFromWarehouse + "(" + SetZeroBeforData(receivedFromwarehouserOrderCount) + ")";
                        string ddd7 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 14 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (startWorkOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment startWorkOrderColor";
                        calenderModel.title = Translation.Start_Work_Order + "(" + SetZeroBeforData(startWorkOrderCount) + ")";
                        string ddd6 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 6 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (changeServiceOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment changeServiceOrderColor";
                        calenderModel.title = Translation.Order_Change_Service + "(" + SetZeroBeforData(changeServiceOrderCount) + ")";
                        string ddd5 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 8 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (appointmentconfirmOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment appointmentconfirmOrderColor";
                        calenderModel.title = Translation.Appointment_Confirmed + "(" + SetZeroBeforData(appointmentconfirmOrderCount) + ")";
                        string ddd4 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 5 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (finishWorkOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment finishWorkOrderColor";
                        calenderModel.title = Translation.Finish_Work + "(" + SetZeroBeforData(finishWorkOrderCount) + ")";
                        string ddd3 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = "/Provider/Finished?StatusId=" + 9 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (onHoldOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment onHoldOrderColor";
                        calenderModel.title = Translation.Order_on_Hold + "(" + SetZeroBeforData(onHoldOrderCount) + ")";
                        string ddd2 = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 7 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    if (appointmentRescheduleOrderCount > 0)
                    {
                        calenderModel = new VmOrderCalendar();
                        calenderModel.className = "i-circle setCalendarLableAlignment appointmentRescheduleOrderColor";
                        calenderModel.title = Translation.Order_Appointment_Reschedule + "(" + SetZeroBeforData(appointmentRescheduleOrderCount) + ")";
                        string ddd = (calenderModel.start = Convert.ToDateTime(item.Substring(0, 10)).ToString("yyyy-MM-dd"));
                        calenderModel.url = baseAddress + "?StatusId=" + 15 + "&supplierId=" + model.SupplierId + "&date=" + item + "&Location=" + model.LocationId;
                        ModelList.Add(calenderModel);
                    }
                    calenderModelList.AddRange(ModelList);
                }
            }
            return calenderModel;
        }

        private static string SetZeroBeforData(int count)
        {
            string countString = string.Empty;
            if (count < 10)
            {
                return "0" + count;
            }
            return count.ToString();
        }

        private List<PublicHoliday> LoadData()
        {
            List<PublicHoliday> lst = new List<PublicHoliday>();
            try
            {
                string line = string.Empty;
                string srcFilePath = "Content/PublicHoliday.txt";
                string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                string fullPath = Path.Combine(rootPath, srcFilePath);
                string filePath = new Uri(fullPath).LocalPath;
                StreamReader sr = new StreamReader(new FileStream(filePath.Replace("\\bin", ""), FileMode.Open, FileAccess.Read));
                while ((line = sr.ReadLine()) != null)
                {
                    PublicHoliday infoObj = new PublicHoliday();
                    string[] info = line.Split(',');
                    infoObj.Sr = Convert.ToInt32(info[0].ToString());
                    infoObj.Title = info[1].ToString();
                    infoObj.Desc = info[2].ToString();
                    infoObj.Start_Date = info[3].ToString();
                    infoObj.End_Date = info[4].ToString();
                    lst.Add(infoObj);
                }
                sr.Dispose();
                sr.Close();
                return lst;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                return lst;
            }
        }

        public ActionResult Calenda1r()
        {
            string password = objSettings.EncryptString("almaneacc", useHashing: false);
            base.Session.Clear();
            return View();
        }

        public JsonResult ChangeCulture(string lang)
        {
            if (lang != null)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }
            return Json(1);
        }

        [HttpPost]
        public JsonResult SetCulture(string culture)
        {
            culture = cultureHelper.GetImplementedCulture(culture);
            base.Response.Cookies.Remove("Syanah_culture");
            HttpCookie cookie = new HttpCookie("Syanah_culture");
            cookie.Value = culture;
            cookie.Expires = DateTime.Now.AddYears(1);
            base.Response.Cookies.Add(cookie);
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            bool IsEnglish = (cls_Defaults.IsEnglish = ((!culture.Equals("ar")) ? true : false));
            base.Session["S_Culture"] = IsEnglish;
            return Json(1);
        }

        public JsonResult UploadLogo(HttpPostedFileBase images, string filePath)
        {
            try
            {
                string strFileName = cls_Defaults.GenerateUniqueId() + Path.GetExtension(images.FileName);
                string pathString = Path.Combine(cls_Defaults.UploadPath, filePath);
                if (!Directory.Exists(pathString))
                {
                    Directory.CreateDirectory(pathString);
                }
                string uploadpath = $"{pathString}\\{strFileName}";
                images.SaveAs(uploadpath);
                return Json(1 + ";" + strFileName);
            }
            catch (Exception)
            {
            }
            return Json(0 + ";");
        }

        public JsonResult UploadLogos(List<HttpPostedFileBase> images, string filePath)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int Count = 0;
                foreach (HttpPostedFileBase item in images)
                {
                    string pathString = Path.Combine(cls_Defaults.UploadPath, filePath);
                    if (!Directory.Exists(pathString))
                    {
                        Directory.CreateDirectory(pathString);
                    }
                    string uploadpath = $"{pathString}\\{item.FileName}";
                    item.SaveAs(uploadpath);
                    Count++;
                }
                output.StatusId = 1;
                output.Data = Count;
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public JsonResult DeleteFile(string fileName, string filePath)
        {
            string pathString = Path.Combine(cls_Defaults.UploadPath, filePath);
            string uploadpath = $"{pathString}\\{fileName}";
            if (System.IO.File.Exists(uploadpath))
            {
                System.IO.File.Delete(uploadpath);
            }
            return Json(1, JsonRequestBehavior.AllowGet);
        }

        public FileResult Download(string FileName, string orderId)
        {
            int OrderId = 0;
            if (!string.IsNullOrEmpty(orderId))
            {
                OrderId = Convert.ToInt32(orderId);
            }
            string path = "C:\\\\inetpub\\\\wwwroot\\\\Najez-Backend-Nodejs\\\\Najez-Backend-ssiva\\\\invoices\\\\" + OrderId + "\\" + FileName;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", FileName);
        }

        public JsonResult CheckFile(string FileName, string orderId)
        {
            int OrderId = 0;
            if (!string.IsNullOrEmpty(orderId))
            {
                OrderId = Convert.ToInt32(orderId);
            }
            string mainPath = HostingEnvironment.ApplicationPhysicalPath;
            string path = "C:\\\\inetpub\\\\wwwroot\\\\Najez-Backend-Nodejs\\\\Najez-Backend-ssiva\\\\invoices\\\\" + OrderId + "\\" + FileName;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            if (fileBytes != null && fileBytes.Any())
            {
                return Json(1);
            }
            return Json(0);
        }

        [HttpPost]
        public async Task<JsonResult> SignIn(string username, string password)
        {
            vm_StatusInfo output = new vm_StatusInfo();
            try
            {
                if (!objUser.CheckUserName(username))
                {
                    output.Status = "Unauthorized ";
                    output.StatusCode = 401;
                    output.Message = "Invalid UserName";
                }
                else if (!(await objUser.CheckPassword(username, password)))
                {
                    output.Status = "Unauthorized ";
                    output.StatusCode = 401;
                    output.Message = "Invalid Password";
                }
                else if (await objUser.Login(username, password) != null)
                {
                    output.Status = "Success";
                    output.StatusCode = 200;
                    output.Message = "Successfully Login";
                }
                else
                {
                    output.Status = "Failure";
                    output.StatusCode = 404;
                    output.Message = "User Not Found";
                }
            }
            catch (Exception ex)
            {
                output.Status = "Failure";
                output.StatusCode = 400;
                output.Message = ex.Message;
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ForgotPassword(string email)
        {
            vm_StatusInfo output = new vm_StatusInfo();
            try
            {
                output = await objUser.ForgotPassword(email);
            }
            catch (Exception ex)
            {
                output.Status = "Failure";
                output.StatusCode = 400;
                output.Message = ex.Message;
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ResetPassword(string email, string otp, string newpassword)
        {
            vm_StatusInfo output = new vm_StatusInfo();
            try
            {
                output = await objUser.ResetPassword(email, otp, newpassword);
            }
            catch (Exception ex)
            {
                output.Status = "Failure";
                output.StatusCode = 400;
                output.Message = ex.Message;
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Login(vm_Login model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                tblAdminUser dt = await objUser.Login(model.UserName, model.Password);
                if (dt != null)
                {
                    _ = dt.UserId;
                    await objUser.UpdateUserLoginStatus(dt.UserId);
                    base.Session[cls_Defaults.Session_IsLabourDriver] = dt.LabourIsDriver;
                    base.Session[cls_Defaults.Session_UserId] = dt.UserId;
                    base.Session[cls_Defaults.Session_UserName] = dt.FirstName + " " + dt.LastName;
                    base.Session[cls_Defaults.Session_UserGroupTypeId] = dt.UserGroupTypeId;
                    base.Session[cls_Defaults.Session_UserGroupId] = dt.UserGroupId;
                    base.Session[cls_Defaults.Session_AccountTypeId] = dt.AccountTypeId;
                    base.Session[cls_Defaults.Session_ProfilePic] = dt.ProfilePic;
                    base.Session[cls_Defaults.Session_HasInventory] = objSettings.HasInventory(dt.UserGroupId);
                    base.Session[cls_Defaults.Session_LaboursNotified] = objSettings.GetLaboursNotified(cls_Defaults.LaboursNotified, dt.UserGroupId);
                    if (dt.tblUserGroupCompany != null)
                    {
                        base.Session[cls_Defaults.Session_UserName] = dt.FirstName + " " + dt.LastName;
                        base.Session[cls_Defaults.Session_CompanyLogo] = dt.tblUserGroupCompany.CompanyLogo;
                        base.Session[cls_Defaults.Session_CompanyNameEN] = dt.tblUserGroupCompany.CompanyNameEN;
                        base.Session[cls_Defaults.Session_CompanyNameAR] = dt.tblUserGroupCompany.CompanyNameAR;
                    }
                    else
                    {
                        base.Session[cls_Defaults.Session_UserName] = "";
                        base.Session[cls_Defaults.Session_CompanyLogo] = "";
                        base.Session[cls_Defaults.Session_CompanyNameEN] = "Admin";
                        base.Session[cls_Defaults.Session_CompanyNameAR] = "Admin";
                        base.Session[cls_Defaults.Session_CompanyLogo] = "";
                    }
                    if (dt.UserGroupTypeId == 7)
                    {
                        output.Message = "/Home/Dashboard2";
                    }
                    else if (dt.AccountTypeId == 17)
                    {
                        output.Message = "Home/Dashboard2";
                    }
                    else if (dt.AccountTypeId == 15)
                    {
                        output.Message = "Orders/Create";
                    }
                    else
                    {
                        output.Message = "/Provider/ServiceProviderDashboard";
                    }
                    output.StatusId = 1;
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> AutoLogin(string Key)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                string val = EncryptDecrypt.Decrypt(Key);
                string UserName = val.Split(';')[0];
                string Password = val.Split(';')[1];
                tblAdminUser dt = await objUser.Login(UserName, Password);
                if (dt != null)
                {
                    int userId = dt.UserId;
                    await objUser.UpdateUserLoginStatus(dt.UserId);
                    FormsAuthentication.SetAuthCookie(userId.ToString(), createPersistentCookie: true);
                    string Value = (string)(output.Data = EncryptDecrypt.Encrypt(UserName + ";" + Password));
                    output.StatusId = 1;
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return Json(output);
        }

        public async Task<JsonResult> GetUserGroupCompanyImage(string Email)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                tblAdminUser dt = await objUser.GetCompanyImage(Email);
                if (dt != null)
                {
                    if (dt.tblUserGroupCompany != null)
                    {
                        string Path = "";
                        if (dt.tblUserGroupCompany.CompanyLogo != null)
                        {
                            Path = cls_Defaults.FindImage(dt.tblUserGroupCompany.CompanyLogo.ToString(), cls_Defaults.CompanyLogo, "");
                        }
                        output.Data = Path;
                        if (!string.IsNullOrEmpty(Path))
                        {
                            output.StatusId = 1;
                        }
                        else
                        {
                            output.StatusId = 0;
                        }
                    }
                    else
                    {
                        output.StatusId = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return Json(output);
        }

        [SiteAuthorize(new string[] { "Provider", "Admin", "Supplier", "SuperAdmin", "SellerStaff", "User" })]
        [SiteAuthorize("Provider", "Admin", "Supplier", "SuperAdmin", "SellerStaff", "User")]
        public ActionResult Dashboard()
        {
            int userGroupId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);
            int accountTypeId = Convert.ToInt32(Session[cls_Defaults.Session_AccountTypeId]);

            if (userGroupId == (int)enumGroupType.Provider)
            {
                if (accountTypeId == (int)enumProviderAcct.Admin)
                    return RedirectToAction("AddUser", "Provider");
                else if (accountTypeId == (int)enumProviderAcct.Supervisor)
                    return RedirectToAction("NewOrders", "Provider");
                else if (accountTypeId == (int)enumProviderAcct.Agent)
                    return RedirectToAction("NewOrders", "Provider");
                else if (accountTypeId == (int)enumProviderAcct.Labour)
                    return RedirectToAction("UnAuthorized", "Home");
                else if (accountTypeId == (int)enumProviderAcct.Driver)
                    return RedirectToAction("UnAuthorized", "Home");
                else
                    return RedirectToAction("NewOrders", "Provider");
            }
            else if (userGroupId == (int)enumGroupType.Supplier)
            {
                if (accountTypeId == (int)enumSupplierAcct.Admin)
                    return RedirectToAction("AddUser", "User");
                else if (accountTypeId == (int)enumSupplierAcct.Executive)
                    return RedirectToAction("Dashboard2", "Home");
                else if (accountTypeId == (int)enumSupplierAcct.CallCenter)
                    return RedirectToAction("Valid", "Orders");
                else if (accountTypeId == (int)enumSupplierAcct.Finance)
                    return RedirectToAction("SupplierInvoice", "Print");
                else if (accountTypeId == (int)enumSupplierAcct.Warehouse)
                    return RedirectToAction("Orders", "Warehouse");
                else if (accountTypeId == (int)enumSupplierAcct.SellerSupervisor)
                    return RedirectToAction("Index", "Orders");
                else if (accountTypeId == (int)enumSupplierAcct.SellerAgent)
                    return RedirectToAction("Create", "Orders");
                return RedirectToAction("Create", "Orders");
            }
            else //if (userGroupId == (int)enumGroupType.Admin)
                return RedirectToAction("Orders", "Setting");
        }


        public ActionResult UnAuthorized()
        {
            return View();
        }

        public async Task<ActionResult> Logout()
        {
            if (base.Session[cls_Defaults.Session_UserId] != null)
            {
                await objUser.UpdateUserLogoutStatus(Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]));
                base.Session.Clear();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<JsonResult> Forgot(vm_Forgot model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objUser.Forgot(model.UserName);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public JsonResult GetOrderCount()
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int TypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int? GroupId = (int?)base.Session[cls_Defaults.Session_UserGroupId];
                if ((int?)base.Session[cls_Defaults.Session_AccountTypeId] == 4)
                {
                    TypeId = 4;
                }
                vm_MenuCount model = (vm_MenuCount)(output.Data = objSettings.GetMenuCount(TypeId, GroupId));
                output.StatusId = 1;
            }
            catch (Exception)
            {
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewOrderCount()
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                if (AccountTypeId == 4)
                {
                    UserGroupTypeId = 4;
                }
                vm_MenuCount model = (vm_MenuCount)(output.Data = objSettings.GetNewOrderCount(UserGroupTypeId, UserGroupId, AccountTypeId, UserId));
                output.StatusId = 1;
            }
            catch (Exception)
            {
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewServiceCount()
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                if (AccountTypeId == 4)
                {
                    UserGroupTypeId = 4;
                }
                vm_MenuCount model = (vm_MenuCount)(output.Data = objSettings.GetNewServiceCount(UserGroupTypeId, UserGroupId, AccountTypeId));
                output.StatusId = 1;
            }
            catch (Exception)
            {
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewComplainCount()
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int AccountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                if (AccountTypeId == 4)
                {
                    UserGroupTypeId = 4;
                }
                vm_MenuCount model = (vm_MenuCount)(output.Data = objSettings.GetNewComplainCount(UserGroupTypeId, UserGroupId, AccountTypeId, UserId));
                output.StatusId = 1;
            }
            catch (Exception)
            {
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorize(new string[] { "Admin", "SellerStaff", "SuperAdmin" })]
        public async Task<ActionResult> Complain(string Id)
        {

            try
            {
                string cultureName = "en-US";
                int LinkId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                tblOrderUserLink userLinks = await objSettings.Get_OrderUserLink(LinkId);
                if (userLinks == null || userLinks.IsActive == false || userLinks.ExpireOn < DateTime.Now)
                {
                    return RedirectToAction("UnAthorized");
                }
                if ((await objSettings.GetOrderById(userLinks.OrderId.Value)).SmsInArabic)
                {
                    cultureName = "ar";
                }
                base.ViewBag.OrderId = Id;
                cultureName = cultureHelper.GetImplementedCulture(cultureName);
                HttpCookie cookie = base.Request.Cookies["Syanah_culture"];
                if (cookie != null)
                {
                    cookie.Value = cultureName;
                }
                else
                {
                    cookie = new HttpCookie("Syanah_culture");
                    cookie.Value = cultureName;
                    cookie.Expires = DateTime.Now.AddYears(1);
                }
                base.Response.Cookies.Add(cookie);
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureName);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
                vm_Complain model = new vm_Complain
                {
                    OrderId = userLinks.OrderId.Value
                };
                List<vm_ComplainType> complainType = await objSettings.GetComplainType();
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

        [SiteAuthorize(new string[] { "Admin", "SellerStaff", "SuperAdmin" })]
        public async Task<ActionResult> ComplainDetail(string Id, string OrderId)
        {
            try
            {
                int complainId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                tblOrderComplain output = await objSettings.GetComplainDetail(complainId);
                base.ViewBag.OrderId = OrderId;
                vm_ComplainDetail model = Mapper.Map<tblOrderComplain, vm_ComplainDetail>(output);
                return View(model);
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [SiteAuthorize(new string[] { "Admin", "SellerStaff", "SuperAdmin" })]
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
                sorts.Add(model.iSortCol_0 == 1, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 0, (tblOrderComplain x) => x.ComplainId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrderComplain> result = objSettings.GetComplains(model.iDisplayLength, PageNo, sorts, filters, 3);
                List<tblOrderComplain> lst = result.Results.ToList();
                List<vm_UserComplainList> output = (List<vm_UserComplainList>)(objResult.Data = Mapper.Map<List<tblOrderComplain>, List<vm_UserComplainList>>(lst));
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

        [HttpPost]
        public async Task<JsonResult> AddComplain(vm_Complain model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.AddComplain(model);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [SiteAuthorize(new string[] { "Provider", "Admin", "Supplier", "SuperAdmin" })]
        public ActionResult ChangePassword()
        {
            int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            vm_EditPassword model = new vm_EditPassword
            {
                UserId = UserId.ToString()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ChangePassword(vm_EditPassword model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int UserId = Convert.ToInt32(model.UserId);
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objUser.UpdateUserPassword(UserId, model.CurrentPassword, model.Password);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Supplier" })]
        public ActionResult Dashboard2(vm_Dashboard obj, string submit)
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [HttpPost]
        public JsonResult GetSupplierDashboardWidgetsData(string StartDate, string EndDate)
        {
            try
            {
                int userId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetSupplierDashboardWidgetsData(StartDate, EndDate, userId, userGroupId, userGroupTypeId, accountTypeId));
                return Json(output);
            }
            catch (Exception ex)
            {
                base.ViewBag.Error = ex.Message;
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetSupplierServiceDistribution(string StartDate, string EndDate)
        {
            try
            {
                int userId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetSupplierServiceDistribution(StartDate, EndDate, userId, userGroupId, userGroupTypeId, accountTypeId));
                return Json(output);
            }
            catch (Exception)
            {
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetSupplierCompleteOrders(string StartDate, string EndDate)
        {
            try
            {
                int userId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetSupplierCompleteOrders(StartDate, EndDate, userId, userGroupId, userGroupTypeId, accountTypeId));
                return Json(output);
            }
            catch (Exception)
            {
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetSupplierInstallationWorkersUtilizationList(string StartDate, string EndDate)
        {
            try
            {
                int userId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int accountTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetSupplierWorkersUtilizationListt(StartDate, EndDate, userId, userGroupId, userGroupTypeId, accountTypeId));
                return Json(output);
            }
            catch (Exception)
            {
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetDashboardWidgetsData(string StartDate, string EndDate)
        {
            try
            {
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetAdminDashboardWidgetsData(StartDate, EndDate, userGroupId, UserGroupTypeId));
                return Json(output);
            }
            catch (Exception ex)
            {
                base.ViewBag.Error = ex.Message;
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetServiceDistribution(string StartDate, string EndDate)
        {
            try
            {
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetAdminServiceDistribution(StartDate, EndDate, userGroupId, UserGroupTypeId));
                return Json(output);
            }
            catch (Exception ex)
            {
                base.ViewBag.Error = ex.Message;
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetCompleteOrders(string StartDate, string EndDate)
        {
            try
            {
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetAdminCompleteOrders(StartDate, EndDate, userGroupId, UserGroupTypeId));
                return Json(output);
            }
            catch (Exception ex)
            {
                base.ViewBag.Error = ex.Message;
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetWorkersUtilizationList(string StartDate, string EndDate)
        {
            try
            {
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetAdminWorkersList(StartDate, EndDate, userGroupId));
                return Json(output);
            }
            catch (Exception ex)
            {
                base.ViewBag.Error = ex.Message;
            }
            return Json(null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;
using Microsoft.CSharp.RuntimeBinder;
using Rotativa;

namespace Almanea.Controllers
{
    public class ProviderController : BaseController
    {
        private bool isEnglish = cls_Defaults.IsEnglish;

        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        private AlmaneaDbEntities _context = new AlmaneaDbEntities();

        private db_Settings objSettings = new db_Settings();

        private db_User objUser = new db_User();

        [SiteAuthorize(new string[] { "Provider" })]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Services()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> CompleteOrder(string OrderId)
        {
            await cls_Sms.CompleteOrder(int.Parse(OrderId));
            return Json(new
            {
                send = 1
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier", "User" })]
        public async Task<ActionResult> GetServices(vm_JqueryDataTables model)
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
                string GroupTyid = base.Session[cls_Defaults.Session_UserGroupId].ToString();
                Filters<tblService> filters = new Filters<tblService>();
                Sorts<tblService> sorts = new Sorts<tblService>();
                filters.Add(CurrentUserId > 0, (tblService x) => x.ServiceProviderId.Contains(GroupTyid));
                sorts.Add(model.iSortCol_0 == 0, (tblService x) => x.ServiceNameEN, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblService x) => x.ServiceNameAR, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblService x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblService> result = objSettings.GetServices(model.iDisplayLength, PageNo, sorts, filters);
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
                int providerid = (int)base.Session[cls_Defaults.Session_UserGroupId];
                List<int> ServiceIds = output.Select((vm_Services o) => o.ServiceId).ToList();
                await objSettings.EditServicesAsSeen(ServiceIds, providerid);
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

        public async Task<ActionResult> GetServiceItems(int Id)
        {
            return Json(new
            {
                data = await objSettings.GetServiceItemById(Id)
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
        public async Task<ActionResult> EditService(string Id)
        {
            vm_ServicesMapper output = new vm_ServicesMapper();
            try
            {
                int providerid = (int)base.Session[cls_Defaults.Session_UserGroupId];
                ViewBag.Items = await objSettings.GetItems2(providerid);
                int id = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                tblServiceMapper result = objSettings.GetServicesMap(id, providerid);
                output.ServiceId = id;
                output.isworking = result.IsWorking;
                output.Estimated = result.Estimated;
                output.ServiceNameEN = result.tblService.ServiceNameEN;
                output.InventoryRequired = result.tblService.InventoryRequired == 1;
            }
            catch (Exception)
            {
            }
            return View(output);
        }

        [HttpPost]
        [SiteAuthorize(new string[] { "Admin", "Executive", "SuperAdmin", "Provider", "Supplier" })]
        public async Task<ActionResult> UpdateService(vm_ServicesMapper model, string[] items, string[] quantity, string InventoryRequired)
        {
            int providerid = (int)base.Session[cls_Defaults.Session_UserGroupId];

            ViewBag.Items = await objSettings.GetItems();
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                if (model.ServiceId > 0)
                {
                    output.Message = Translation.ReqAll;
                    bool Required = ((InventoryRequired == "on") ? true : false);
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await objSettings.EditServiceMapper(model.ServiceId, model.Estimated, providerid, model.isworking, items, quantity, Required);
                }
            }
            catch (Exception)
            {
            }
            return RedirectToAction("Services");
        }

        public ActionResult ProviderAgent()
        {
            base.ViewBag.Message = base.TempData["ShowMessage"];
            base.ViewBag.DirectionList = objSettings.GetDirection().ToList();
            base.TempData.Remove("ShowMessage");
            return View();
        }

        public JsonResult ProviderAgentGetData(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int StatusId, int Direction = 0)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int AgentId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(Direction > 0, (tblOrder x) => x.tblLocation.Direction == (int?)Direction);
                filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
                int mins = Convert.ToInt32(objSettings.GetSettingByKey("ordershowduration"));
                DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                filters.Add(condition: true, (tblOrder x) => x.AddedDate <= now5Min);
                if (!string.IsNullOrEmpty(InstallDate))
                {
                    DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
                }
                filters.Add(condition: true, (tblOrder x) => (x.Status == 1 && !x.tblOrderHistories.Any((tblOrderHistory y) => y.OrderId == x.OrderId && y.ServiceProviderId == (int?)ProviderId && y.Status == 3)) || (x.ReservedProvider == ProviderId && x.Status != 10 && x.Status != 9 && x.Status != 12 && x.Status != 11));
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                List<int> getService = objSettings.getServiceByProvide(ProviderId);
                filters.Add(condition: true, (tblOrder x) => x.tblOrderServices.All((tblOrderService k) => getService.Contains(k.ServiceId)));
                Page<tblOrder> result = objSettings.GetOrdersOnlySP(model.iDisplayLength, PageNo, sorts, filters, ProviderId, AgentId);
                List<tblOrder> lst = result.Results.ToList();
                lst.Remove(lst.Where((tblOrder a) => a.Status == 3 || a.Status == 12).FirstOrDefault());
                List<vm_OrderList> output = (List<vm_OrderList>)(objResult.Data = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst));
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
        public async Task<JsonResult> AutoDispatch(string[] rows_selected)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int result = 0;
                foreach (string item in rows_selected)
                {
                    int OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(item));
                    result = await objSettings.ReserveOrderByAutodispatch(OrderId);
                }
                if (result == 1)
                {
                    output.Data = "Resrved Successfully !!";
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [SiteAuthorize(new string[] { "Provider", "Admin", "Executive", "SuperAdmin", "SellerStaff" })]
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 1)]
        public ActionResult NewOrders(int statusId = 0, int supplierId = 0, string date = "", int location = 0)
        {
            int AccountType = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
            base.ViewBag.DirectionList = objSettings.GetDirection().ToList();
            FilterDropDown model = new FilterDropDown();
            model.StatusId = statusId;
            model.SupplierId = supplierId;
            if (date != string.Empty)
            {
                model.Date = date.Substring(0, 10);
            }
            model.LocationId = location;
            base.ViewBag.Message = base.TempData["ShowMessage"];
            base.ViewBag.DirectionList = objSettings.GetDirection().ToList();
            vm_Settings settingsdata = objSettings.GetSettin1g();
            base.ViewBag.autodispatch = settingsdata.Autodispatch;
            base.TempData.Remove("ShowMessage");
            base.ViewBag.ShowHideP = objSettings.GetAdminPage(1, 1, AccountType).ToList();
            base.ViewBag.ShowHideC = objSettings.GetAdminPage(1, 2, AccountType).ToList();
            return View(model);
        }

        public JsonResult GetNewOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int StatusId, int Direction = 0)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int AgentId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                if (InstallDate != string.Empty)
                {
                    InstallDate = InstallDate.Substring(0, 10);
                }
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(Direction > 0, (tblOrder x) => x.tblLocation.Direction == (int?)Direction);
                filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = objSettings.GetOrdersOnlySP(model.iDisplayLength, PageNo, sorts, filters, ProviderId, AgentId);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item in output)
                {
                    int status = objSettings.GetAssignedStatus(int.Parse(item.OrderNo));
                    tblProviderTimeSlot timeslot = objSettings.GetTimeslot(int.Parse(item.OrderNo));
                    vm_Locations location = objSettings.GetLocation(item.LocationId);
                    item.Location = location.LocationNameEN;
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

        public JsonResult GetNewOrders1(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int StatusId, int Direction = 0)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int AgentId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == ProviderId);
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(Direction > 0, (tblOrder x) => x.tblLocation.Direction == (int?)Direction);
                filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
                int mins = Convert.ToInt32(objSettings.GetSettingByKey("ordershowduration"));
                DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                filters.Add(condition: true, (tblOrder x) => x.AddedDate <= now5Min);
                if (!string.IsNullOrEmpty(InstallDate))
                {
                    DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
                }
                filters.Add(condition: true, (tblOrder x) => (x.Status == 1 && !x.tblOrderHistories.Any((tblOrderHistory y) => y.OrderId == x.OrderId && y.ServiceProviderId == (int?)ProviderId && y.Status == 3)) || (x.ReservedProvider == ProviderId && x.Status != 10 && x.Status != 9 && x.Status != 12 && x.Status != 11));
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item2 in output)
                {
                    vm_Locations location = objSettings.GetLocation(item2.LocationId);
                    tblProviderTimeSlot timeslot = objSettings.GetTimeslot(int.Parse(item2.OrderNo));
                    item2.Location = location.LocationNameEN;
                    if (timeslot != null)
                    {
                        item2.InstallDate = item2.InstallDate.ToString().Substring(0, 10) + "<br/>" + cls_Defaults.get12hour(timeslot.StartHour.Value, timeslot.EndHour.Value);
                    }
                    else
                    {
                        item2.InstallDate = item2.InstallDate.ToString().Substring(0, 10);
                    }
                }
                lst.Remove(lst.Where((tblOrder a) => a.Status == 3 || a.Status == 12).FirstOrDefault());
                IQueryable<tblAdminUser> getagentspovider = _context.tblAdminUsers;
                IEnumerable<int> datalist = from x in getagentspovider.Where((tblAdminUser x) => x.UserGroupId == (int?)ProviderId).ToList()
                                            select x.UserId;
                List<int?> objdata = new List<int?>();
                foreach (int item in datalist)
                {
                    objdata.Add(item);
                }
                List<int?> accounttypeid = new List<int?> { 6, 7 };
                IEnumerable<int?> slectedSPorderlistid = from x in _context.OrderDisplays.Where((OrderDisplay x) => x.ReservedBy == null || objdata.Contains(x.ReservedBy)).ToList()
                                                         select x.OrderId;
                lst = lst.Where((tblOrder x) => slectedSPorderlistid.Contains(x.OrderId)).ToList();
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

        [HttpPost]
        public async Task<JsonResult> CheckOrderStatus(string Id)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                output.StatusId = (await objSettings.VerifyOrder(OrderId)).StatusId;
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public async Task<ActionResult> AssignDriver(string Id)
        {
            int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            tblOrder order = await objSettings.GetOrder(Convert.ToInt32(EncryptDecrypt.Decrypt(Id)));
            objSettings.GetLaboursAssigned(Convert.ToInt32(EncryptDecrypt.Decrypt(Id)));
            vm_OrderStatus model = new vm_OrderStatus
            {
                OrderId = order.OrderId,
                DriverId = order.DriverId.Value,
                LabourId = order.LabourId.ToString()
            };
            IEnumerable<SelectListItem> drivers = (await objUser.GetDrivers(userGroupId)).Select((tblAdminUser s) => new SelectListItem
            {
                Text = s.FirstName + " " + s.LastName + "###Driver",
                Value = s.UserId.ToString()
            });
            IEnumerable<SelectListItem> laboursandDrivers = (await objUser.GetLaboursAndDrivers(userGroupId)).Select((tblAdminUser s) => new SelectListItem
            {
                Text = s.FirstName + " " + s.LastName + "###Labour and Driver",
                Value = s.UserId.ToString()
            });
            base.ViewBag.Drivers = drivers.Union(laboursandDrivers);
            return PartialView("_AssignDriver", model);
        }

        public async Task<ActionResult> AssignLabour(string Id)
        {
            int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            tblOrder order = await objSettings.GetOrder(Convert.ToInt32(EncryptDecrypt.Decrypt(Id)));
            vm_OrderStatus model = new vm_OrderStatus
            {
                OrderId = order.OrderId,
                DriverId = order.DriverId.Value,
                LabourId = order.LabourId.ToString()
            };
            IEnumerable<SelectListItem> labours = (await objUser.GetAssignLabours(userGroupId, Convert.ToDateTime(order.InstallDate), order.OrderId)).Select((tblAdminUser s) => new SelectListItem
            {
                Text = s.FirstName + " " + s.LastName,
                Value = s.UserId.ToString()
            });
            base.ViewBag.Labours = labours;
            return PartialView("_AssignLabour", model);
        }

        [SiteAuthorize(new string[] { "Provider", "Admin", "Executive", "SuperAdmin", "SellerSupervisor", "Supplier" })]
        public async Task<ActionResult> Details(string Id)
        {
            try
            {
                bool CanEdit = false;
                base.ViewBag.Vat = objSettings.Vat();
                base.ViewBag.Id = Id;
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                vm_Order model = await objSettings.GetOrderById(OrderId);
                if (model != null)
                {
                    byte status = model.Status;
                    int reservedProvider = model.ReservedProvider;
                    if (DateTime.TryParse(model.dtInstallDate.ToString(), out var _))
                    {
                        model.InstallDate = DateTime.Parse(model.dtInstallDate.ToString()).ToString("dd/MM/yyyy", cls_Defaults.DateTimeCulture);
                    }
                    if (userGroupTypeId == 1 && status >= 2 && reservedProvider != userGroupId)
                    {
                        base.TempData["ShowMessage"] = Translation.OrderAlreadyReserved;
                        return RedirectToAction("NewOrders");
                    }
                    if (userGroupTypeId == 1 && status == 11)
                    {
                        base.TempData["ShowMessage"] = Translation.OrderDeleted;
                        return RedirectToAction("NewOrders");
                    }
                    if (userGroupTypeId == 1 && model.IsOnEdit)
                    {
                        base.TempData["ShowMessage"] = Translation.OrderNotReady;
                        return RedirectToAction("NewOrders");
                    }
                    if (userGroupTypeId == (int)enumGroupType.Admin)
                    {
                        base.ViewBag.IsAdmin = true;
                    }

                    ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);
                    tblProviderTimeSlot ordertimeslot = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)model.OrderId).FirstOrDefault();
                    if (ordertimeslot != null)
                    {
                        string slot = cls_Defaults.get12hour(ordertimeslot.StartHour.Value, ordertimeslot.EndHour.Value);
                        base.ViewBag.ordertimeslot = slot;
                    }
                    List<vm_OrderHistoryList> History = await objSettings.GetHistory(OrderId);
                    foreach (vm_OrderHistoryList item in History)
                    {
                        if (item.Status == (int)OrderStatus.PartialDelivery)
                        {
                            item.DoneBy = objSettings.GetLabourorDriveNamebyId(model.DriverId);
                        }
                    }
                    vm_Order OrderHistory = await objSettings.GetOrderById(OrderId);
                    ViewBag.Additional = await objSettings.GetAdditional(OrderId);
                    ViewBag.AdditionalAR = await objSettings.GetAdditionalAR(OrderId);

                    if (History.Any(x => x.Status == (byte)OrderStatus.Finish))
                    {
                        vm_OrderHistoryList hist = History.FirstOrDefault((vm_OrderHistoryList x) => x.Status == 9 && !string.IsNullOrEmpty(x.FileAttachment));
                        if (hist != null && !string.IsNullOrEmpty(hist.FileAttachment))
                        {
                            base.ViewBag.ExistAttachment = hist.FileAttachment;
                        }
                        if (userGroupTypeId == 1)
                        {
                            CanEdit = History.Any((vm_OrderHistoryList x) => x.Status == 9 || x.Status == 10);
                        }
                    }
                    else if (OrderHistory.Status == 9)
                    {
                        CanEdit = true;
                    }
                    IEnumerable<SelectListItem> drivers = (await objUser.GetDrivers(userGroupId)).Select((tblAdminUser s) => new SelectListItem
                    {
                        Text = s.FirstName + " " + s.LastName + "  (" + Translation.Driver + ")",
                        Value = s.UserId.ToString()
                    });
                    IEnumerable<SelectListItem> labours = (await objUser.GetAssignLabours(userGroupId, Convert.ToDateTime(model.InstallDate), OrderId)).Select((tblAdminUser s) => new SelectListItem
                    {
                        Text = s.FirstName + " " + s.LastName,
                        Value = s.UserId.ToString()
                    });
                    IEnumerable<SelectListItem> laboursandDrivers = (await objUser.GetLaboursAndDrivers(userGroupId)).Select((tblAdminUser s) => new SelectListItem
                    {
                        Text = s.FirstName + " " + s.LastName,
                        Value = s.UserId.ToString()
                    });
                    base.ViewBag.Labours = labours;
                    base.ViewBag.Drivers = drivers.Union(laboursandDrivers);
                    if (userGroupTypeId != 1)
                    {
                        base.ViewBag.History = History.ToList();
                    }
                    else
                    {
                        base.ViewBag.History = History.Where((vm_OrderHistoryList p) => p.Status != 3).ToList();
                    }
                    base.ViewBag.CanEdit = CanEdit;
                    int canFinish = 0;
                    if (model.dtInstallDate.HasValue && model.dtInstallDate <= DateTime.Today)
                    {
                        canFinish = 1;
                    }
                    base.ViewBag.IsEnglish = IsEnglish;
                    base.ViewBag.CanFinish = canFinish;
                    base.ViewBag.DriverID = model.DriverId.ToString();
                    base.ViewBag.LabourID = model.LabourId.ToString();
                    base.ViewBag.ServiceProviderStatus = objUser.GetUserIsActive((int)base.Session[cls_Defaults.Session_UserId]);
                    return View(model);
                }
                return RedirectToAction("NewOrders");
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [AllowAnonymous]
        public ActionResult PrintDetails(string Id)
        {
            return new ActionAsPdf("Details", new { Id });
        }

        [SiteAuthorize(new string[] { "Provider", "Supplier", "SuperAdmin" })]
        public ActionResult Completed()
        {
            base.ViewBag.Status = 10;
            base.ViewBag.Title = Translation.CompletedOrders;
            base.ViewBag.PageDesc = Translation.CompletedOrderDesc;
            return View("Index");
        }

        [SiteAuthorize(new string[] { "Provider", "Supplier", "SuperAdmin" })]
        public ActionResult Finished(int statusId = 0, int supplierId = 0, string date = "", int location = 0)
        {
            base.ViewBag.Status = 9;
            base.ViewBag.SupplierId = supplierId;
            if (string.IsNullOrEmpty(date))
            {
                base.ViewBag.InstallDate = "";
            }
            else
            {
                base.ViewBag.InstallDate = Convert.ToDateTime(date).ToShortDateString();
            }
            base.ViewBag.LocationId = location;
            base.ViewBag.Title = Translation.FinishedOrders;
            base.ViewBag.PageDesc = Translation.FinishOrderDesc;
            return View("FinishOrderList");
        }

        [SiteAuthorize(new string[] { "Provider", "Supplier", "SuperAdmin" })]
        public ActionResult Cancelled()
        {
            base.ViewBag.Status = 11;
            base.ViewBag.Title = Translation.CancelledOrders;
            base.ViewBag.PageDesc = Translation.CancelOrderDesc;
            return View("Index");
        }

        [SiteAuthorize(new string[] { "Provider", "Supplier", "SuperAdmin" })]
        public ActionResult Rejected(int statusId = 0, int supplierId = 0, string date = "", int location = 0)
        {
            base.ViewBag.Status = statusId;
            base.ViewBag.Supplier = supplierId;
            base.ViewBag.Date = date;
            base.ViewBag.Location = location;
            base.ViewBag.Title = Translation.CancelledOrders;
            base.ViewBag.PageDesc = Translation.CancelOrderDesc;
            return View("Index");
        }

        public JsonResult GetOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                Page<tblOrder> result = orderDataList(model, Seller, Customer, InvoiceNo, InstallDate, Location, Status, StatusId);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item in output)
                {
                    tblProviderTimeSlot timeslot = objSettings.GetTimeslot(int.Parse(item.OrderNo));
                    vm_Locations location = objSettings.GetLocation(item.LocationId);
                    item.Location = location.LocationNameEN;
                    if (timeslot != null)
                    {
                        item.InstallDate = item.InstallDate.ToString().Substring(0, 10) + "<br/>" + cls_Defaults.get12hour(timeslot.StartHour.Value, timeslot.EndHour.Value);
                    }
                    else
                    {
                        item.InstallDate = item.InstallDate.ToString().Substring(0, 10);
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

        public JsonResult GetOrders2(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                Page<tblOrder> result = orderDataList2(model, Seller, Customer, InvoiceNo, InstallDate, Location, Status, StatusId);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = (List<vm_OrderList>)(objResult.Data = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst));
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

        public JsonResult GetCancelandRejectOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                Page<tblOrder> result = CancelandRejectedOrderDataList(model, Seller, Customer, InvoiceNo, InstallDate, Location, Status, StatusId);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = (List<vm_OrderList>)(objResult.Data = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst));
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

        private Page<tblOrder> orderDataList(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId)
        {
            int ProviderId = (int)base.Session[cls_Defaults.Session_UserGroupId];
            int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            int AccountType = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
            int PageNo = 1;
            if (model.iDisplayStart >= model.iDisplayLength)
            {
                PageNo = model.iDisplayStart / model.iDisplayLength + 1;
            }
            Filters<tblOrder> filters = new Filters<tblOrder>();
            Sorts<tblOrder> sorts = new Sorts<tblOrder>();
            filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
            filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
            filters.Add(Status > 0, (tblOrder x) => x.Status == Status);
            filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
            if (!string.IsNullOrEmpty(InstallDate))
            {
                DateTime date = DateTime.ParseExact(Convert.ToDateTime(InstallDate).ToShortDateString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
            }
            if (!string.IsNullOrEmpty(InvoiceNo))
            {
                if (int.TryParse(InvoiceNo, out var i) && i > 1000)
                {
                    int orderId = Convert.ToInt32(i);
                    filters.Add(condition: true, (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId == orderId);
                }
                else
                {
                    filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo));
                }
            }
            if (UserGroupTypeId == 1 && AccountType == 20)
            {
                if (Status == 9)
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == 10 || x.Status == 9);
                }
                if (Status == 3)
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == 3);
                }
                if (Status == 12)
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == 12 || x.Status == 3);
                }
                if (Status == 11)
                {
                    filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == 11 || x.Status == 3 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                }
            }
            else
            {
                if (Status == 9)
                {
                    filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == 10 || x.Status == 9 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                }
                if (Status == 3)
                {
                    filters.Add(condition: true, (tblOrder x) => (x.ReservedProvider == ProviderId && x.Status == 3) || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status));
                }
                if (Status == 12)
                {
                    filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == 12 || x.Status == 3 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                }
                if (Status == 11)
                {
                    filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == 11 || x.Status == 3 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                }
                else
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == Status && x.ReservedProvider == ProviderId);
                }
            }
            sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
            Page<tblOrder> result = new Page<tblOrder>();
            if (UserGroupTypeId == 1 && AccountType == 20)
            {
                return objSettings.GetOrdersOnlySP(model.iDisplayLength, PageNo, sorts, filters, ProviderId, 0);
            }
            return objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);
        }

        private Page<tblOrder> orderDataList2(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId)
        {
            int ProviderId = (int)base.Session[cls_Defaults.Session_UserGroupId];
            int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            int AccountType = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
            int PageNo = 1;
            if (model.iDisplayStart >= model.iDisplayLength)
            {
                PageNo = model.iDisplayStart / model.iDisplayLength + 1;
            }
            Filters<tblOrder> filters = new Filters<tblOrder>();
            Sorts<tblOrder> sorts = new Sorts<tblOrder>();
            filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
            filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
            filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
            filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
            if (!string.IsNullOrEmpty(InstallDate))
            {
                DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
            }
            if (!string.IsNullOrEmpty(InvoiceNo))
            {
                if (int.TryParse(InvoiceNo, out var i) && i > 1000)
                {
                    int orderId = Convert.ToInt32(i);
                    filters.Add(condition: true, (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId == orderId);
                }
                else
                {
                    filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo));
                }
            }
            if (UserGroupTypeId == 1 && AccountType == 20)
            {
                if (Status == 9)
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == 10 || x.Status == 9);
                }
                if (Status == 3)
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == 3);
                }
                if (Status == 12)
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == 12);
                }
            }
            else
            {
                if (Status == 9)
                {
                    filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == 10 || x.Status == 9 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                }
                if (Status == 3)
                {
                    filters.Add(condition: true, (tblOrder x) => (x.ReservedProvider == ProviderId && x.Status == 3) || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status));
                }
                if (Status == 12)
                {
                    filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == 12 || x.Status == 3 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                }
                else
                {
                    filters.Add(condition: true, (tblOrder x) => x.Status == Status && x.ReservedProvider == ProviderId);
                }
            }
            sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
            Page<tblOrder> result = new Page<tblOrder>();
            if (UserGroupTypeId == 1 && AccountType == 20)
            {
                return objSettings.GetOrdersOnlySP(model.iDisplayLength, PageNo, sorts, filters, ProviderId, 0);
            }
            return objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);
        }

        private Page<tblOrder> CancelandRejectedOrderDataList(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int Status, int StatusId)
        {
            int ProviderId = (int)base.Session[cls_Defaults.Session_UserGroupId];
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
                DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
            }
            if (!string.IsNullOrEmpty(InvoiceNo))
            {
                if (int.TryParse(InvoiceNo, out var i) && i > 1000)
                {
                    int orderId = Convert.ToInt32(i);
                    filters.Add(condition: true, (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId == orderId);
                }
                else
                {
                    filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo));
                }
            }
            if (Status == 9)
            {
                filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == ProviderId && (x.Status == Status || x.Status == 10 || x.Status == 9 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
            }
            sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
            sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
            return objSettings.GetCancelandRejectedOrders(model.iDisplayLength, PageNo, sorts, filters);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateStatus(vm_OrderStatus model, int LabourID, int DriverID)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                model.DriverId = DriverID;
                model.LabourId = LabourID.ToString();
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                if (userGroupTypeId == 1)
                {
                    vm_jsOutput order = await this.objSettings.VerifyOrderActive(model.OrderId);
                    if (order != null && order.StatusId == -2)
                    {
                        return Json(output);
                    }
                    if (order != null && order.StatusId == 1)
                    {
                        model.Status = (byte)order.StatusId;
                    }
                }
                if (model.Status == 4 || model.Status == 8 || model.Status == 9)
                {
                    output.StatusId = 1;
                    return Json(output);
                }
                if (model.Status == 2)
                {
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await this.objSettings.ReserveOrder(model);
                }
                else if (model.Status == 13 || model.Status == 20)
                {
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await this.objSettings.ReleaseOrder(model);
                }
                else
                {
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await this.objSettings.UpdateHistory(model);
                }
                if (output.StatusId == 1)
                {
                    vm_Order obj = new vm_Order
                    {
                        OrderId = model.OrderId,
                        Comments = model.Comments,
                        Status = model.Status,
                        DriverId = model.DriverId
                    };
                    await this.objSettings.EditOrderStatus(obj);
                }
                if (output.StatusId > 0 && model.Status == 5)
                {
                    db_Settings objSettings = new db_Settings();
                    Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
                }
                if (output.StatusId > 0 && model.Status == 14)
                {
                    db_Settings objSettings2 = new db_Settings();
                    int IsProoduction = Convert.ToInt32((await objSettings2.GetSetting()).IsProoduction);
                    if (IsProoduction == 1)
                    {
                        await cls_PushNotification.PushNotificationToLabour(model.OrderId);
                    }
                }
                if (output.StatusId == -3)
                {
                    output.Message = Translation.OrderAlreadyReserved;
                }
                else if (output.StatusId == -2)
                {
                    output.Message = Translation.OrderDeleted;
                }
                if (model.Status == 4)
                {
                    await MergeStepsStatus(model);
                }
                output.StatusId = model.Status;
                if (model.Status == 2)
                {
                    db_Settings objSettings3 = new db_Settings();
                    int calcva = 0;
                    new List<int>();
                    objSettings3.GetProviderSetting(userGroupTypeId).FirstOrDefault();
                    int userGroupTypeId3 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                    vm_Order order2 = await objSettings3.GetOrderById(model.OrderId);
                    List<int?> splistdistributed = objSettings3.GetSpOrderDistributed(model.OrderId);
                    if (splistdistributed.Count > 0)
                    {
                        foreach (int? itemspids in splistdistributed)
                        {
                            if (itemspids != userGroupTypeId3)
                            {
                                int dailcapacity = 0;
                                int spids = Convert.ToInt32(itemspids);
                                List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(itemspids));
                                tblSetting wokrhrs = objSettings3.GetEorkinhHrsysettings(spids);
                                if (labours.Count > 0 && labours != null && labours.Count > 0)
                                {
                                    dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
                                }
                                List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == model.OrderId).ToList();
                                foreach (tblOrderService item in OrderServicssse)
                                {
                                    tblServiceMapper spassignedservice = objSettings3.GetServicesmap2(Convert.ToInt32(spids), item.ServiceId);
                                    int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                                    calcva += item.Quantity * spestimat / 60;
                                }
                                DateTime orderdatatt = Convert.ToDateTime(order2.InstallDate);
                                tblTeamCapacityCalculation model2 = (from x in _context.tblTeamCapacityCalculations
                                                                     where x.ServiceProviderId == (int?)spids && x.InstallDate == orderdatatt && x.OrderId == (int?)model.OrderId
                                                                     orderby x.Id descending
                                                                     select x).FirstOrDefault();
                                if (model2 != null)
                                {
                                    int totaloccperctcapcitycurent = Convert.ToInt32(model2.CurrentCapacity);
                                    int consumedcap = Convert.ToInt32(model2.ConsumedCapacity);
                                    new tblTeamCapacityCalculation();
                                    model2.Updatedate = DateTime.Now;
                                    model2.InstallDate = Convert.ToDateTime(order2.InstallDate);
                                    model2.OrderId = order2.OrderId;
                                    model2.ServiceProviderId = spids;
                                    model2.DailyCapacity = dailcapacity;
                                    model2.ConsumedCapacity = consumedcap - calcva;
                                    model2.CurrentCapacity = totaloccperctcapcitycurent + consumedcap;
                                    double curavailabe2 = (double)model2.CurrentCapacity.Value / (double)model2.DailyCapacity.Value * 100.0;
                                    if (curavailabe2 < 0.0)
                                    {
                                        curavailabe2 = 0.0;
                                    }
                                    model2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                                    _context.Entry(model2).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }
                            }
                            calcva = 0;
                        }
                    }
                }
                if (model.Status == 3 || model.Status == 7 || model.Status == 9 || model.Status == 12)
                {
                    int userGroupTypeId2 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                    tblTeamCapacityCalculation tblTeamCapacity2 = (from x in _context.tblTeamCapacityCalculations
                                                                   where x.ServiceProviderId == (int?)userGroupTypeId2 && x.OrderId == (int?)model.OrderId
                                                                   orderby x.Id descending
                                                                   select x).FirstOrDefault();
                    tblTeamCapacity2.CurrentCapacity += ((!tblTeamCapacity2.ConsumedCapacity.HasValue) ? new int?(0) : tblTeamCapacity2.ConsumedCapacity);
                    tblTeamCapacity2.ConsumedCapacity = 0;
                    tblTeamCapacity2.ServiceProviderId = userGroupTypeId2;
                    tblTeamCapacity2.Updatedate = DateTime.Now;
                    double curavailabe = (double)tblTeamCapacity2.CurrentCapacity.Value / (double)tblTeamCapacity2.DailyCapacity.Value * 100.0;
                    if (curavailabe < 0.0)
                    {
                        curavailabe = 0.0;
                    }
                    tblTeamCapacity2.CapcityPercentage = Convert.ToDecimal(curavailabe);
                    _context.Entry(tblTeamCapacity2).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                if (model.Status == 9)
                {
                    _context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)model.OrderId).ToList();
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public async Task<JsonResult> MergeStepsStatus(vm_OrderStatus model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                model.Status = 5;
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                model.InstallDate = (await this.objSettings.GetOrderReservedHistory(model.OrderId)).tblOrder.InstallDate.ToString();
                if (model.Status == 4 || model.Status == 8 || model.Status == 9)
                {
                    output.StatusId = 1;
                    return Json(output);
                }
                if (userGroupTypeId == 1)
                {
                    vm_jsOutput order = await this.objSettings.VerifyOrderActive(model.OrderId);
                    if (order != null && order.StatusId == -2)
                    {
                        return Json(output);
                    }
                }
                if (model.Status == 2)
                {
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await this.objSettings.ReserveOrder(model);
                }
                else if (model.Status == 13)
                {
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await this.objSettings.ReleaseOrder(model);
                }
                else
                {
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await this.objSettings.UpdateHistory(model);
                }
                if (output.StatusId == 1)
                {
                    vm_Order obj = new vm_Order
                    {
                        OrderId = model.OrderId,
                        Comments = model.Comments,
                        Status = model.Status,
                        DriverId = model.DriverId
                    };
                    await this.objSettings.EditOrderStatus(obj);
                }
                if (output.StatusId > 0 && model.Status == 5)
                {
                    db_Settings objSettings = new db_Settings();
                    int IsProoduction = Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
                    if (IsProoduction != 1)
                    {
                        await cls_Sms.ConfirmOrder(model.OrderId);
                        await cls_PushNotification.PushNotificationToDriver(model.OrderId);
                    }
                    else
                    {
                        await cls_Sms.ConfirmOrder(model.OrderId);
                        await cls_PushNotification.PushNotificationToDriver(model.OrderId);
                    }
                }
                if (output.StatusId == -1)
                {
                    output.Message = Translation.OrderAlreadyReserved;
                }
                else if (output.StatusId == -2)
                {
                    output.Message = Translation.OrderDeleted;
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [HttpPost]
        [Route("TeamCapacityReschedule/{OrderId1}")]
        public async Task<JsonResult> TeamCapacityReschedule(string OrderId1)
        {
            var recordsdate = new[]
            {
            new
            {
                dates = "",
                value = ""
            }
        }.ToList();
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                string OrderId2 = base.TempData["OrderId"].ToString();
                int orderid1 = Convert.ToInt32(OrderId2);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                db_User objUser = new db_User();
                db_Settings objSettings = new db_Settings();
                int calcva = 0;
                List<int> add_list = new List<int>();
                List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == orderid1).ToList();
                foreach (tblOrderService item2 in OrderServicssse)
                {
                    tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(userGroupTypeId), item2.ServiceId);
                    int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                    calcva += item2.Quantity * spestimat / 60;
                }
                tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(userGroupTypeId));
                int dailcapacity = 0;
                List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(userGroupTypeId));
                if (labours.Count > 0 && labours != null && labours.Count > 0)
                {
                    dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
                }
                if (dailcapacity >= Convert.ToInt32(calcva))
                {
                    output.Message = "true";
                    base.ViewBag.Capaicity = 2;
                    add_list.Add(1);
                    int spidd2 = Convert.ToInt32(userGroupTypeId);
                    List<IGrouping<DateTime?, tblTeamCapacityCalculation>> model2 = (from x in db.tblTeamCapacityCalculations
                                                                                     where x.ServiceProviderId == (int?)spidd2 && x.tblProviderTimeSlot.OrderId == x.OrderId && x.tblProviderTimeSlot.ServiceProviderId == x.ServiceProviderId && x.InstallDate > DateTime.UtcNow
                                                                                     select x into c
                                                                                     group c by c.InstallDate).ToList();
                    if (model2.Count > 0)
                    {
                        new vm_installdateblock();
                        foreach (IGrouping<DateTime?, tblTeamCapacityCalculation> iteminstaldate2 in model2)
                        {
                            tblTeamCapacityCalculation totaloccperct2 = iteminstaldate2.OrderByDescending((tblTeamCapacityCalculation x) => x.Id).FirstOrDefault();
                            iteminstaldate2.Count();
                            objSettings.Getpercensetting(Convert.ToInt32(userGroupTypeId));
                            if (totaloccperct2.CurrentCapacity < Convert.ToInt32(calcva))
                            {
                                vm_installdateblock modeldd2 = new vm_installdateblock();
                                modeldd2.date = totaloccperct2.InstallDate.ToString();
                                recordsdate.Add(new
                                {
                                    dates = modeldd2.date.ToString(),
                                    value = "true"
                                });
                                base.ViewBag.Capaicity = 1;
                                output.Message = "Out Capacity";
                            }
                        }
                    }
                }
                else if (dailcapacity <= Convert.ToInt32(calcva))
                {
                    output.Message = "In Capacity case 2";
                    objSettings.GetOrdersInstaldate(Convert.ToInt32(userGroupTypeId));
                    int spidd = Convert.ToInt32(userGroupTypeId);
                    List<IGrouping<DateTime?, tblTeamCapacityCalculation>> model = (from x in db.tblTeamCapacityCalculations
                                                                                    where x.ServiceProviderId == (int?)spidd && x.InstallDate > DateTime.UtcNow
                                                                                    select x into c
                                                                                    group c by c.InstallDate).ToList();
                    if (model.Count > 0)
                    {
                        new vm_installdateblock();
                        foreach (IGrouping<DateTime?, tblTeamCapacityCalculation> iteminstaldate in model)
                        {
                            tblTeamCapacityCalculation totaloccperct = iteminstaldate.OrderByDescending((tblTeamCapacityCalculation x) => x.Id).FirstOrDefault();
                            iteminstaldate.Count();
                            tblSetting getsettingper = objSettings.Getpercensetting(Convert.ToInt32(userGroupTypeId));
                            decimal? capcityPercentage = totaloccperct.CapcityPercentage;
                            decimal num = Convert.ToInt32(getsettingper.KeyValue);
                            if ((capcityPercentage.GetValueOrDefault() < num) & capcityPercentage.HasValue)
                            {
                                vm_installdateblock modeldd = new vm_installdateblock();
                                modeldd.date = totaloccperct.InstallDate.ToString();
                                recordsdate.Add(new
                                {
                                    dates = modeldd.date.ToString(),
                                    value = "true"
                                });
                                base.ViewBag.Capaicity = 1;
                                output.Message = "Out Capacity";
                            }
                            capcityPercentage = totaloccperct.CapcityPercentage;
                            num = Convert.ToInt32(getsettingper.KeyValue);
                            if ((capcityPercentage.GetValueOrDefault() >= num) & capcityPercentage.HasValue)
                            {
                                foreach (var item in from c in recordsdate
                                                     group c by c.dates)
                                {
                                    foreach (var item3 in item)
                                    {
                                        recordsdate.RemoveAll(x => x.dates == item3.dates.ToString());
                                    }
                                }
                            }
                            else
                            {
                                base.ViewBag.Capaicity = 2;
                                output.Message = "Out Capacity";
                            }
                        }
                    }
                    base.ViewBag.Capaicity = 5;
                    add_list.Add(3);
                }
                else
                {
                    output.Message = "Out Capacity";
                    base.ViewBag.Capaicity = 1;
                }
                foreach (int item4 in add_list)
                {
                    _ = item4;
                    _ = 1;
                }
                if (add_list.Contains(1))
                {
                    output.Message = "true";
                }
                else if (add_list.Contains(2))
                {
                    output.Message = "In Capacity case 1";
                }
                else if (add_list.Contains(3))
                {
                    output.Message = "In Capacity case 2";
                }
            }
            catch (Exception)
            {
            }
            recordsdate = recordsdate.Skip(1).ToList();
            base.ViewBag.blockdatess = recordsdate;
            output.Data = recordsdate;
            return Json(output);
        }

        public async Task<ActionResult> ChangeStatusTeams(string Id, string status)
        {
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            vm_OrderStatus model = new vm_OrderStatus
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };
            base.ViewBag.OrderId = model.OrderId;
            tblOrder src = await objSettings.GetOrder(model.OrderId);
            model.InstallDate = ((src.PreferDate == 2 && src.InstallDate.HasValue) ? src.InstallDate.Value.ToString("dd/MM/yyyy") : "");
            List<SelectListItem> Mlabours = (from s in objUser.GetAssignedLabours(userGroupTypeId4, Convert.ToDateTime(model.InstallDate), 0, model.OrderId)
                                             select new SelectListItem
                                             {
                                                 Text = s.FirstName + " " + s.LastName,
                                                 Value = s.UserId.ToString()
                                             }).ToList();
            base.ViewBag.Labours = Mlabours;
            return PartialView("_ChangeStatusTeams", model);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeStatusTeams(vm_OrderStatus model, int[] LabourId, int status)
        {
            vm_jsOutput output = new vm_jsOutput();
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            vm_jsOutput vm_jsOutput = output;
            vm_jsOutput.StatusId = await objSettings.EditBulkOrderStatus(model, LabourId, status);
            base.ViewBag.OrderId = model.OrderId;
            tblOrder src = await objSettings.GetOrder(model.OrderId);
            model.InstallDate = ((src.PreferDate == 2 && src.InstallDate.HasValue) ? src.InstallDate.Value.ToString("dd/MM/yyyy") : "");
            List<SelectListItem> Mlabours = (from s in objUser.GetAavilableLabours(userGroupTypeId4, Convert.ToDateTime(model.InstallDate), 0, model.OrderId)
                                             select new SelectListItem
                                             {
                                                 Text = s.FirstName + " " + s.LastName,
                                                 Value = s.UserId.ToString()
                                             }).ToList();
            base.ViewBag.Labours = Mlabours;
            return Json(output);
        }

        public async Task<ActionResult> ReSchedule(string Id, string abc)
        {
            vm_OrderSchedule model = new vm_OrderSchedule
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };
            base.TempData["OrderId"] = model.OrderId;
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            base.Session["OrderId"] = model.OrderId;
            tblOrder src = await objSettings.GetOrder(model.OrderId);
            model.InstallDate = ((src.PreferDate == 2 && src.InstallDate.HasValue) ? src.InstallDate.Value.ToString("dd/MM/yyyy") : "");
            tblAdminUser labours = objUser.GetAavilableLabours(userGroupTypeId4, Convert.ToDateTime(model.InstallDate), 0, model.OrderId).FirstOrDefault();
            if (labours != null)
            {
                model.LabourId = new List<int>();
                model.LabourId.Add(labours.UserId);
            }
            List<tblAdminUser> drivers = objUser.GetAvailDrivers(userGroupTypeId4);
            if (drivers.Count == 0)
            {
                drivers = objUser.GetAvailLabourAndDrivers(userGroupTypeId4);
            }
            bool.Parse(base.Session[cls_Defaults.Session_IsLabourDriver].ToString());
            model.DriverId = drivers.FirstOrDefault().UserId;
            int calcva = 0;
            List<SelectListItem> services = new List<SelectListItem>();
            List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == model.OrderId).ToList();
            int i = 0;
            foreach (tblOrderService item in OrderServicssse)
            {
                if (isEnglish)
                {
                    services.Insert(i, new SelectListItem
                    {
                        Value = item.ServiceId.ToString(),
                        Text = item.tblService.ServiceNameEN + "," + item.Quantity
                    });
                }
                else
                {
                    services.Insert(i, new SelectListItem
                    {
                        Value = item.ServiceId.ToString(),
                        Text = item.tblService.ServiceNameAR + "," + item.Quantity
                    });
                }
                i++;
                tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(userGroupTypeId4), item.ServiceId);
                int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                if (item.Quantity * spestimat / 60 < 1)
                {
                    calcva = 1;
                }
                calcva += item.Quantity * spestimat / 60;
            }
            model.TotalServiceTime = calcva;
            Gettimeslot(model.InstallDate.ToString());
            List<SelectListItem> Mlabours = (from s in objUser.GetAavilableLabours(userGroupTypeId4, Convert.ToDateTime(model.InstallDate), 0, model.OrderId)
                                             select new SelectListItem
                                             {
                                                 Text = s.FirstName + " " + s.LastName,
                                                 Value = s.UserId.ToString()
                                             }).ToList();
            base.ViewBag.LabourId2 = Mlabours;
            base.ViewBag.services = services;
            return PartialView("_ReSchedule", model);
        }

        [HttpPost]
        public async Task<JsonResult> Reschedule(vm_OrderSchedule model, IEnumerable<string> labourIds, string[] Isleader, string[] Quantity, int[] ServiceIds, string SPrefferMeridian, int LabourIdAuto = 0, int DriveridAuto = 0, int Driverid = 0)
        {
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            string multiLabours = "";
            if (model.LabourId2 == null)
            {
                model.LabourId2 = new List<int>();
            }
            int i = 0;
            objSettings.RemoveOrderAssigned(model.OrderId);
            objSettings.RemoveOrderAssigned(model.OrderId);
            vm_jsOutput output = new vm_jsOutput();
            foreach (string oneLabourId in labourIds)
            {
                if (base.Session[cls_Defaults.Session_HasInventory] != null && base.Session[cls_Defaults.Session_HasInventory].ToString() == "1")
                {
                    Inventory_Master InventoryMaster = objSettings.GetInventoryMasterLabour(int.Parse(oneLabourId));
                    if (InventoryMaster == null)
                    {
                      //  output.Message = Translation.ReqInventory;
                        return Json(output);
                    }
                    int InventoryDetails = objSettings.GetInventoryDetailsLabour(oneLabourId);
                    int consumedAmount = InventoryDetails;
                    _ = InventoryMaster.Quantity.Value - consumedAmount;
                }
                objSettings.AddEditOrderAssigned(int.Parse(oneLabourId), model.Status, model.OrderId, "0", ServiceIds[i], Quantity[i], userGroupTypeId4);
                i++;
            }
            if (model.LabourId == null)
            {
                model.LabourId = new List<int>();
                foreach (string labourId in labourIds)
                {
                    model.LabourId.Add(int.Parse(labourId));
                    multiLabours = labourId + ",";
                }
            }
            if (LabourIdAuto > 0)
            {
                model.LabourId[0] = LabourIdAuto;
            }
            if (Driverid > 0)
            {
                Driverid = DriveridAuto;
                model.DriverId = objSettings.GetUser(model.LabourId[0]);
                if (model.DriverId == -1)
                {
                    List<tblAdminUser> drivers = objUser.GetAvailDrivers(userGroupTypeId4);
                    model.DriverId = drivers.FirstOrDefault().UserId;
                }
            }
            try
            {
                model.PrefferMeridian = Convert.ToByte(SPrefferMeridian);
                vm_jsOutput order = await objSettings.VerifyOrderActive(model.OrderId);
                if (order != null && order.StatusId == -2)
                {
                    return Json(order);
                }
                if (model.PreferDate == 2 && string.IsNullOrEmpty(model.InstallDate))
                {
                    output.Message = Translation.ReqInstallDate;
                    return Json(order);
                }
                vm_Order ord = new vm_Order
                {
                    OrderId = model.OrderId,
                    Comments = model.Comments,
                    Status = 5,
                    DriverId = model.DriverId.Value,
                    LabourIds = model.LabourId
                };
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.Reschedule(model);
                await objSettings.EditOrderStatus(ord);
                vm_OrderStatus obj = new vm_OrderStatus
                {
                    OrderId = model.OrderId,
                    Comments = model.Comments,
                    LabourId = multiLabours,
                    DriverId = model.DriverId.Value,
                    TimeSlot = model.TimeSlot,
                    InstallDate = model.InstallDate
                };
                await MergeStepsStatus(obj);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> OrderCheck(FormCollection formCollection)
        {
            new List<string>();
            new List<string>();
            int i = 0;
            Dictionary<string, int> data = new Dictionary<string, int>();
            Dictionary<string, int> Adata = new Dictionary<string, int>();
            int orderId = int.Parse(formCollection["OrderId"].ToString());
            while (formCollection["ServiceIds[" + i + "]"] != null)
            {
                int key = int.Parse(formCollection["ServiceIds[" + i + "]"]);
                int value = int.Parse(formCollection["Quantity[" + i + "]"]);
                data.Add(key.ToString() + i, value);
                i++;
            }
            vm_jsOutput output = new vm_jsOutput();
            for (int ii2 = 0; ii2 < data.Keys.Count; ii2++)
            {
                KeyValuePair<string, int> item = data.ElementAt(ii2);
                if (item.Key == "-11")
                {
                    output.StatusId = -1;
                    output.Message = "you have to select a service";
                    return Json(output);
                }
                string j = item.Key.Remove(item.Key.Length - 1);
                _ = item.Key;
                if (Adata.ContainsKey(j))
                {
                    Adata[j] += item.Value;
                }
                else
                {
                    Adata.Add(j, item.Value);
                }
            }
            try
            {
                output.StatusId = 1;
                for (int ii = 0; ii < Adata.Keys.Count; ii++)
                {
                    KeyValuePair<string, int> item2 = Adata.ElementAt(ii);
                    int orderQuantity = objSettings.getorderquantitybyService(orderId, int.Parse(item2.Key.ToString()));
                    if (string.IsNullOrEmpty(item2.Value.ToString()) || item2.Value == 0)
                    {
                        output.StatusId = -1;
                        output.Message = "The Order service quantity should be more than 0";
                        return Json(output);
                    }
                    if (item2.Value > orderQuantity)
                    {
                        output.StatusId = -1;
                        output.Message = "The Order service quantity is bigger than the actual one";
                        return Json(output);
                    }
                    if (orderQuantity != item2.Value)
                    {
                        output.StatusId = -1;
                        output.Message = "The Order service must be same as service quantity";
                        return Json(output);
                    }
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> OrderCalculate(FormCollection formCollection)
        {
            new List<string>();
            new List<string>();
            int i = 0;
            Dictionary<string, int> data = new Dictionary<string, int>();
            Dictionary<string, int> Adata = new Dictionary<string, int>();
            int orderId = int.Parse(formCollection["OrderId"].ToString());
            while (formCollection["ServiceIds[" + i + "]"] != null)
            {
                int key = int.Parse(formCollection["ServiceIds[" + i + "]"]);
                if (formCollection["Quantity[" + i + "]"] != null && formCollection["Quantity[" + i + "]"] != "")
                {
                    int value = int.Parse(formCollection["Quantity[" + i + "]"]);
                    data.Add(key.ToString() + i, value);
                }
                else
                {
                    data.Add(key.ToString() + i, 0);
                }
                i++;
            }
            vm_jsOutput output = new vm_jsOutput();
            for (int ii2 = 0; ii2 < data.Keys.Count; ii2++)
            {
                KeyValuePair<string, int> item = data.ElementAt(ii2);
                if (item.Key == "-11")
                {
                    output.StatusId = -1;
                    output.Message = "you have to select a service";
                    return Json(output);
                }
                string j = item.Key.Remove(item.Key.Length - 1);
                _ = item.Key;
                if (Adata.ContainsKey(j))
                {
                    Adata[j] += item.Value;
                }
                else
                {
                    Adata.Add(j, item.Value);
                }
            }
            try
            {
                output.StatusId = 1;
                for (int ii = 0; ii < Adata.Keys.Count; ii++)
                {
                    KeyValuePair<string, int> item2 = Adata.ElementAt(ii);
                    int orderQuantity = objSettings.getorderquantitybyService(orderId, int.Parse(item2.Key.ToString()));
                    if (item2.Value > orderQuantity)
                    {
                        output.StatusId = -1;
                        output.Message = "The Order service quantity is bigger than the actual one";
                        return Json(output);
                    }
                    if (item2.Value < orderQuantity)
                    {
                        output.StatusId = -1;
                        output.Data = orderQuantity - item2.Value;
                        output.Message = "The remaining quantity for this service is " + (orderQuantity - item2.Value);
                        return Json(output);
                    }
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public async Task<ActionResult> AppointmentReSchedule(string Id)
        {
            vm_OrderSchedule model = new vm_OrderSchedule
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };
            base.TempData["OrderId"] = model.OrderId;
            base.Session["OrderId"] = model.OrderId;
            tblOrder src = await objSettings.GetOrder(model.OrderId);
            model.InstallDate = ((src.PreferDate == 2 && src.InstallDate.HasValue) ? src.InstallDate.Value.ToString("dd/MM/yyyy") : "");
            base.TempData["OrderId"] = model.OrderId;
            base.ViewBag.GetDates = "ANAYA";
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            int calcva = 0;
            List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == model.OrderId).ToList();
            List<SelectListItem> services = new List<SelectListItem>();
            int i = 0;
            foreach (tblOrderService item in OrderServicssse)
            {
                if (isEnglish)
                {
                    services.Insert(i, new SelectListItem
                    {
                        Value = item.ServiceId.ToString(),
                        Text = item.tblService.ServiceNameEN + "," + item.Quantity
                    });
                }
                else
                {
                    services.Insert(i, new SelectListItem
                    {
                        Value = item.ServiceId.ToString(),
                        Text = item.tblService.ServiceNameAR + "," + item.Quantity
                    });
                }
                i++;
                tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(userGroupTypeId4), item.ServiceId);
                int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                if (item.Quantity * spestimat / 60 < 1)
                {
                    calcva = 1;
                }
                calcva += item.Quantity * spestimat / 60;
            }
            model.TotalServiceTime = calcva;
            Gettimeslot(model.InstallDate.ToString());
            List<SelectListItem> labours = (from s in objUser.GetAavilableLabours(userGroupTypeId4, Convert.ToDateTime(model.InstallDate), 0, model.OrderId)
                                            select new SelectListItem
                                            {
                                                Text = s.FirstName + " " + s.LastName,
                                                Value = s.UserId.ToString()
                                            }).ToList();
            base.ViewBag.LabourId2 = labours;
            base.ViewBag.services = services;
            return PartialView("_AppointmentReSchedule", model);
        }

        [Route("Gettimeslot/?abc={abc}")]
        public JsonResult Gettimeslot(string abc)
        {
            tblOrder model = objSettings.GetOrderTimeslot(Convert.ToInt32(base.Session["OrderId"]));
            List<SelectListItem> model2 = new List<SelectListItem>();
            try
            {
                var recordsdate = new[]
                {
                new
                {
                    start = "",
                    end = ""
                }
            }.ToList();
                int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int spestimat = 0;
                int calcva = 0;
                List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == model.OrderId).ToList();
                foreach (tblOrderService item2 in OrderServicssse)
                {
                    tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(userGroupTypeId4), item2.ServiceId);
                    spestimat = Convert.ToInt32(spassignedservice.Estimated);
                    calcva = ((item2.Quantity * spestimat / 60 < 1) ? 1 : (calcva + item2.Quantity * spestimat / 60));
                }
                DateTime? dd = model.InstallDate;
                if (calcva >= 24)
                {
                    base.ViewBag.GetTimeSLOT = null;
                }
                else
                {
                    if (calcva < 12)
                    {
                        if (IsEnglish)
                        {
                            model2.Insert(0, new SelectListItem
                            {
                                Value = "-1",
                                Text = "-- Please select a time slot --"
                            });
                        }
                        else
                        {
                            model2.Insert(0, new SelectListItem
                            {
                                Value = "-1",
                                Text = "-- اختر الوقت المفضل --"
                            });
                        }
                    }
                    else
                    {
                        model2.Insert(0, new SelectListItem
                        {
                            Value = "-1",
                            Text = "-- Please select start time --"
                        });
                    }
                    int calt = calcva;
                    string s = DateTime.Now.ToString("hh");
                    DateTime DDDD = Convert.ToDateTime(abc).Date;
                    int startval = 0;
                    if (DDDD.Date.ToString("dd/MM/yyyy") == DateTime.UtcNow.Date.ToString("dd/MM/yyyy"))
                    {
                        startval = ((DateTime.UtcNow.Hour > 8) ? DateTime.UtcNow.Hour : 0);
                    }
                    DateTime orderdda = Convert.ToDateTime(model.InstallDate);
                    List<tblProviderTimeSlot> modeltimeslot = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.InstallDate == DDDD).ToList();
                    db_Settings obj = new db_Settings();
                    tblSetting wokrhrs = obj.GetEorkinhHrsysettings(userGroupTypeId4);
                    if (wokrhrs != null)
                    {
                        Convert.ToInt32(wokrhrs.KeyValue);
                        _ = 0;
                    }
                    int endhour = 9 + Convert.ToInt32(wokrhrs.KeyValue);
                    if (modeltimeslot.Count > 0)
                    {
                        vm_installdateblock modeldd = new vm_installdateblock();
                        foreach (tblProviderTimeSlot iteminstaldate in modeltimeslot)
                        {
                            modeldd = new vm_installdateblock();
                            recordsdate.Add(new
                            {
                                start = iteminstaldate.StartHour.ToString(),
                                end = iteminstaldate.EndHour.ToString()
                            });
                            int k = 0;
                            recordsdate = recordsdate.Skip(1).ToList();
                            int indva = ((startval == 0) ? 9 : startval);
                            for (int j = indva; j <= endhour; j++)
                            {
                                int gettc2 = 0;
                                int gettcstart = 0;
                                gettcstart = Convert.ToInt32(j) + k;
                                gettc2 = gettcstart + calt;
                                if (gettcstart >= endhour || gettc2 >= endhour)
                                {
                                    break;
                                }
                                int start2 = 0;
                                int end2 = 0;
                                bool exist2 = false;
                                int index2 = 0;
                                start2 = gettcstart;
                                end2 = gettc2;
                                if (gettcstart > 12)
                                {
                                    start2 = gettcstart - 12;
                                }
                                if (gettc2 > 12)
                                {
                                    end2 = gettc2 - 12;
                                }
                                foreach (tblProviderTimeSlot item3 in modeltimeslot)
                                {
                                    if (item3.StartHour == gettcstart)
                                    {
                                        exist2 = true;
                                        index2++;
                                    }
                                }
                                List<tblAdminUser> labourssp3 = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)userGroupTypeId4 && x.AccountTypeId == 10).ToList();
                                if ((index2 == labourssp3.Count) ? true : false)
                                {
                                    string existtext3 = gettcstart + "-" + gettc2;
                                    if (model2.Any((SelectListItem cus) => cus.Value == existtext3))
                                    {
                                        continue;
                                    }
                                    List<tblAdminUser> labourssp2 = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)userGroupTypeId4 && x.AccountTypeId == 10).ToList();
                                    List<tblProviderTimeSlot> starthrscount2 = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.InstallDate == iteminstaldate.InstallDate && x.StartHour == (int?)gettcstart).ToList();
                                    tblOrder getords2 = objSettings.GetOrderTimeslot(Convert.ToInt32(iteminstaldate.OrderId));
                                    foreach (tblAdminUser itemlab2 in labourssp2.ToList())
                                    {
                                        tblProviderTimeSlot getords3 = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.LabourId == (int?)itemlab2.UserId && x.InstallDate == iteminstaldate.InstallDate).FirstOrDefault();
                                        if (getords3 == null || itemlab2.UserId != getords3.LabourId)
                                        {
                                            continue;
                                        }
                                        tblProviderTimeSlot modeltimeslot3 = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.OrderId == iteminstaldate.OrderId).FirstOrDefault();
                                        if (gettcstart == modeltimeslot3.StartHour || endhour == modeltimeslot3.EndHour)
                                        {
                                            k = Convert.ToInt32(modeltimeslot3.TotalConsumedHour);
                                            labourssp2.RemoveAll((tblAdminUser d) => d.UserId == getords3.LabourId);
                                            starthrscount2.RemoveAll((tblProviderTimeSlot d) => d.LabourId == getords3.LabourId);
                                        }
                                    }
                                    if (labourssp2.Count <= starthrscount2.Count && labourssp2.Count <= 0)
                                    {
                                        continue;
                                    }
                                    k = 0;
                                    string today3 = DateTime.Now.Date.ToString("dd/MM/yyyy");
                                    if (abc == today3)
                                    {
                                        if (gettc2 >= DateTime.Now.Hour && gettcstart > DateTime.Now.Hour)
                                        {
                                            model2.Add(new SelectListItem
                                            {
                                                Value = existtext3,
                                                Text = start2 + ":00" + ((gettcstart < 12) ? " AM" : " PM") + "-" + end2 + ":00" + ((gettc2 < 12) ? " AM" : " PM")
                                            });
                                        }
                                    }
                                    else
                                    {
                                        model2.Add(new SelectListItem
                                        {
                                            Value = existtext3,
                                            Text = start2 + ":00" + ((gettcstart < 12) ? " AM" : " PM") + "-" + end2 + ":00" + ((gettc2 < 12) ? " AM" : " PM")
                                        });
                                    }
                                    continue;
                                }
                                string existtext2 = gettcstart + "-" + gettc2;
                                string[] items2 = existtext2.Split('-');
                                string a2 = items2[0];
                                string b2 = items2[1];
                                if (model2.Any((SelectListItem cus) => cus.Value == existtext2))
                                {
                                    continue;
                                }
                                List<tblAdminUser> labourssp = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)userGroupTypeId4 && x.AccountTypeId == 10).ToList();
                                List<tblProviderTimeSlot> starthrscount = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.InstallDate == iteminstaldate.InstallDate && x.StartHour == (int?)gettcstart).ToList();
                                tblOrder getords = objSettings.GetOrderTimeslot(Convert.ToInt32(iteminstaldate.OrderId));
                                foreach (tblAdminUser itemlab1 in labourssp.ToList())
                                {
                                    tblProviderTimeSlot getords4 = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.LabourId == (int?)itemlab1.UserId && x.InstallDate == iteminstaldate.InstallDate).FirstOrDefault();
                                    if (getords4 == null || itemlab1.UserId != getords.LabourId)
                                    {
                                        continue;
                                    }
                                    tblProviderTimeSlot modeltimeslot2 = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.OrderId == iteminstaldate.OrderId).FirstOrDefault();
                                    if (gettcstart == modeltimeslot2.StartHour || endhour == modeltimeslot2.EndHour)
                                    {
                                        k = Convert.ToInt32(modeltimeslot2.TotalConsumedHour);
                                        labourssp.RemoveAll((tblAdminUser d) => d.UserId == getords4.LabourId);
                                        starthrscount.RemoveAll((tblProviderTimeSlot d) => d.LabourId == getords4.LabourId);
                                    }
                                }
                                if (labourssp.Count <= starthrscount.Count && labourssp.Count <= 0)
                                {
                                    continue;
                                }
                                k = 0;
                                string today2 = DateTime.Now.Date.ToString("dd/MM/yyyy");
                                if (abc == today2)
                                {
                                    if (gettc2 >= DateTime.Now.Hour && gettcstart > DateTime.Now.Hour)
                                    {
                                        model2.Add(new SelectListItem
                                        {
                                            Value = existtext2,
                                            Text = start2 + ":00" + ((gettcstart < 12) ? " AM" : " PM") + "-" + end2 + ":00" + ((gettc2 < 12) ? " AM" : " PM")
                                        });
                                    }
                                }
                                else
                                {
                                    model2.Add(new SelectListItem
                                    {
                                        Value = existtext2,
                                        Text = start2 + ":00" + ((gettcstart < 12) ? " AM" : " PM") + "-" + end2 + ":00" + ((gettc2 < 12) ? " AM" : " PM")
                                    });
                                }
                            }
                            k = 0;
                        }
                    }
                    else
                    {
                        int start = 0;
                        int end = 0;
                        recordsdate = recordsdate.Skip(1).ToList();
                        for (int i = 9; i <= endhour; i++)
                        {
                            int gettc = 0;
                            int gettcstart2 = 0;
                            gettcstart2 = Convert.ToInt32(i);
                            gettc = gettcstart2 + calt;
                            if (calcva < 12)
                            {
                                if (gettcstart2 >= endhour || gettc >= endhour)
                                {
                                    break;
                                }
                            }
                            else if (gettcstart2 >= endhour)
                            {
                                break;
                            }
                            start = gettcstart2;
                            end = gettc;
                            if (gettcstart2 > 12)
                            {
                                start = gettcstart2 - 12;
                            }
                            if (gettc > 12)
                            {
                                end = gettc - 12;
                            }
                            bool exist = false;
                            foreach (tblProviderTimeSlot item in modeltimeslot)
                            {
                                if (item.StartHour == gettcstart2)
                                {
                                    exist = true;
                                }
                                if (item.EndHour >= gettcstart2 && item.StartHour <= gettcstart2)
                                {
                                    exist = true;
                                }
                            }
                            if (exist)
                            {
                                continue;
                            }
                            string existtext = gettcstart2 + "-" + gettc;
                            string[] items = existtext.Split('-');
                            string a = items[0];
                            string b = items[1];
                            if (model2.Any((SelectListItem cus) => cus.Text == existtext))
                            {
                                continue;
                            }
                            string today = DateTime.Now.Date.ToString("dd/MM/yyyy");
                            if (abc == today)
                            {
                                if (gettc >= DateTime.Now.Hour && gettcstart2 > DateTime.Now.Hour)
                                {
                                    if (calcva >= 12)
                                    {
                                        model2.Add(new SelectListItem
                                        {
                                            Value = existtext,
                                            Text = start + ":00" + ((gettcstart2 < 12) ? " AM" : " PM")
                                        });
                                        continue;
                                    }
                                    model2.Add(new SelectListItem
                                    {
                                        Value = existtext,
                                        Text = start + ":00" + ((gettcstart2 < 12) ? " AM" : " PM") + "-" + end + ":00" + ((gettc < 12) ? " AM" : " PM")
                                    });
                                }
                            }
                            else if (calcva >= 12)
                            {
                                model2.Add(new SelectListItem
                                {
                                    Value = existtext,
                                    Text = start + ":00" + ((gettcstart2 < 12) ? " AM" : " PM")
                                });
                            }
                            else
                            {
                                model2.Add(new SelectListItem
                                {
                                    Value = existtext,
                                    Text = start + ":00" + ((gettcstart2 < 12) ? " AM" : " PM") + "-" + end + ":00" + ((gettc < 12) ? " AM" : " PM")
                                });
                            }
                        }
                    }
                    base.ViewBag.GetTimeSLOT = model2;
                }
            }
            catch (Exception)
            {
                throw;
            }
            model2?.Select((SelectListItem x) => new SelectListItem
            {
                Value = x.Value.ToString(),
                Text = x.Text.ToString()
            }).ToList();
            return Json(model2, JsonRequestBehavior.AllowGet);
        }

        [Route("GetBookedSlot/?abc={abc}&abc2={abc2}")]
        public JsonResult GetBookedSlot(string abc, string abc2)
        {
            DateTime DDDD = Convert.ToDateTime(abc2);
            int startval = 0;
            string[] items = abc.Split('-');
            string a = items[0];
            string b = items[1];
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            tblOrder model = objSettings.GetOrderTimeslot(Convert.ToInt32(base.Session["OrderId"]));
            string availslot = "";
            int starthrs = Convert.ToInt32(a);
            List<SelectListItem> model2 = new List<SelectListItem>();
            try
            {
                tblProviderTimeSlot slotbooked = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.LabourId == (int?)model.LabourId && x.InstallDate.Value == DDDD.Date && x.StartHour == (int?)starthrs).FirstOrDefault();
                availslot = ((slotbooked == null) ? "0" : "1");
            }
            catch (Exception)
            {
                throw;
            }
            return Json(availslot, JsonRequestBehavior.AllowGet);
        }

        [Route("GetAvailDriver")]
        public JsonResult GetAvailDriver()
        {
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            List<SelectListItem> drivers = (from s in objUser.GetAvailDrivers(userGroupTypeId4)
                                            select new SelectListItem
                                            {
                                                Text = s.FirstName + " " + s.LastName,
                                                Value = s.UserId.ToString()
                                            }).ToList();
            return Json(drivers, JsonRequestBehavior.AllowGet);
        }

        [Route("GetAvilLaborDrp/?abc={abc}&abc2={abc2}")]
        public JsonResult GetAvilLaborDrp(string abc, string abc2)
        {
            DateTime DDDD = Convert.ToDateTime(abc2);
            int startval = 0;
            string[] items = abc.Split('-');
            string a = items[0];
            string b = items[1];
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            tblOrder scr = objSettings.GetOrderTimeslot(Convert.ToInt32(base.Session["OrderId"]));
            tblOrder model = scr;
            string availslot = "";
            int starthrs = Convert.ToInt32(a);
            List<SelectListItem> model2 = new List<SelectListItem>();
            List<SelectListItem> labours = (from s in objUser.GetAavilableLabours(userGroupTypeId4, Convert.ToDateTime(DDDD), starthrs)
                                            select new SelectListItem
                                            {
                                                Text = s.FirstName + " " + s.LastName,
                                                Value = s.UserId.ToString()
                                            }).ToList();
            return Json(labours, JsonRequestBehavior.AllowGet);
        }

        [Route("GetLaborAvailbleSlot/?abc={abc}")]
        public JsonResult GetLaborAvailbleSlot1(string abc, string abc2)
        {
            DateTime DDDD = Convert.ToDateTime(abc2);
            int startval = 0;
            string[] items = abc.Split('-');
            string a = items[0];
            string b = items[1];
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            tblOrder model = objSettings.GetOrderTimeslot(Convert.ToInt32(base.Session["OrderId"]));
            string availslot = "";
            int starthrs = Convert.ToInt32(a);
            List<SelectListItem> model2 = new List<SelectListItem>();
            try
            {
                tblProviderTimeSlot slotbooked = db.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.ServiceProviderId == (int?)userGroupTypeId4 && x.LabourId == (int?)model.LabourId && x.InstallDate.Value == DDDD.Date && x.StartHour == (int?)starthrs).FirstOrDefault();
                availslot = ((slotbooked == null) ? "labour available" : "labour already booked");
            }
            catch (Exception)
            {
                throw;
            }
            return Json(availslot, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetLaborAvailbleSlot(string abc, string abc2)
        {
            vm_OrderSchedule model = new vm_OrderSchedule();
            base.TempData["OrderId"] = model.OrderId;
            await objSettings.GetOrder(model.OrderId);
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            List<SelectListItem> labours = (from s in objUser.GetAavilableLabours(userGroupTypeId4, Convert.ToDateTime(model.InstallDate), 0)
                                            select new SelectListItem
                                            {
                                                Text = s.FirstName + " " + s.LastName,
                                                Value = s.UserId.ToString()
                                            }).ToList();
            base.ViewBag.LabourId2 = labours;
            base.ViewBag.GetDates = model.InstallDate;
            return PartialView("_AppointmentReSchedule", model);
        }

        public async Task<ActionResult> AppointmentReSchedule1(string Id)
        {
            vm_OrderSchedule model = new vm_OrderSchedule
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };
            base.TempData["OrderId"] = model.OrderId;
            tblOrder src = await objSettings.GetOrder(model.OrderId);
            model.InstallDate = ((src.PreferDate == 2 && src.InstallDate.HasValue) ? src.InstallDate.Value.ToString("dd/MM/yyyy") : "");
            base.ViewBag.GetDates = model.InstallDate;
            return PartialView("_AppointmentReSchedule", model);
        }

        [HttpPost]
        public async Task<JsonResult> AppointmentReSchedule(vm_OrderSchedule model, int? LaboursID, int? DriversID, string SPrefferMeridian, string[] Isleader, string[] Quantity, int[] ServiceIds, IEnumerable<string> labourIds)
        {
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            if (model.LabourId2 == null)
            {
                model.LabourId2 = new List<int>();
            }
            int i = 0;
            foreach (string oneLabourId in labourIds)
            {
                this.objSettings.AddEditOrderAssigned(int.Parse(oneLabourId), model.Status, model.OrderId, "0", ServiceIds[i], Quantity[i], userGroupTypeId4);
                i++;
            }
            if (model.LabourId == null)
            {
                model.LabourId = new List<int>();
                foreach (string labourId in labourIds)
                {
                    model.LabourId.Add(int.Parse(labourId));
                    _ = labourId + ",";
                }
            }
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                model.PrefferMeridian = Convert.ToByte(SPrefferMeridian);
                vm_jsOutput order = await this.objSettings.VerifyOrderActive(model.OrderId);
                if (order != null && order.StatusId == -2)
                {
                    return Json(output);
                }
                if (model.PreferDate == 2 && string.IsNullOrEmpty(model.InstallDate))
                {
                    output.Message = Translation.ReqInstallDate;
                    return Json(output);
                }
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await this.objSettings.AppointmentReschedule(model);
                vm_Order obj = new vm_Order
                {
                    OrderId = model.OrderId,
                    Comments = model.Comments,
                    Status = 15,
                    DriverId = DriversID.Value,
                    LabourIds = model.LabourId
                };
                if (output.StatusId > 0)
                {
                    await this.objSettings.EditOrderStatus(obj);
                }
                obj.Status = 15;
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                if (output.StatusId > 0 && obj.Status == 15)
                {
                    db_Settings objSettings = new db_Settings();
                    int IsProoduction = Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
                    if (IsProoduction != 1)
                    {
                        await cls_Sms.ConfirmOrder(model.OrderId);
                        await cls_PushNotification.PushNotificationToDriver(model.OrderId);
                    }
                    else
                    {
                        await cls_Sms.ConfirmOrder(model.OrderId);
                        await cls_PushNotification.PushNotificationToDriver(model.OrderId);
                    }
                }
                if (output.StatusId == -1)
                {
                    output.Message = Translation.OrderAlreadyReserved;
                }
                else if (output.StatusId == -2)
                {
                    output.Message = Translation.OrderDeleted;
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public ActionResult ChangeService(string Id)
        {
            base.ViewBag.Vat = objSettings.Vat();
            int OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
            base.ViewBag.OrderId = OrderId;
            return PartialView("_ChangeService");
        }

        [HttpPost]
        public async Task<JsonResult> ChangeService(List<vm_OrderServices> model, string Comments, decimal Vat, decimal Total)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int orderId = model[0].OrderId;
                vm_jsOutput order = await objSettings.VerifyOrderActive(orderId);
                if (order != null && order.StatusId == -2)
                {
                    return Json(output);
                }
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.NewOrderService(model, Comments, Vat, Total);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public ActionResult FileAttach(string Id)
        {

            vm_FileAttach model = new vm_FileAttach
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };

            return PartialView("_FileAttach", model);
        }

        [HttpPost]
        public async Task<JsonResult> FileAttach(vm_FileAttach model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                if (UserGroupTypeId == 1)
                {
                    vm_jsOutput order = await objSettings.VerifyOrderActive(model.OrderId);
                    if (order != null && order.StatusId == -2)
                    {
                        return Json(output);
                    }
                }
                if (string.IsNullOrEmpty(model.InvoiceImage))
                {
                    output.Message = Translation.UploadAttachment;
                    return Json(output);
                }
                vm_OrderStatus attach = new vm_OrderStatus
                {
                    OrderId = model.OrderId,
                    InvoiceImage = model.InvoiceImage,
                    Comments = model.Comments,
                    Status = 9
                };
                int canUpdater = objSettings.CanUpdateAttachment(attach);
                if (canUpdater == 1)
                {
                    attach.Status = 9;
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await objSettings.UpdateAttachment(attach);
                }
                else
                {
                    attach.Status = 9;
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await objSettings.UpdateHistory(attach);
                }
                if (output.StatusId == 1)
                {
                    await cls_Sms.CompleteOrder(model.OrderId);
                    await cls_Email.FinishOrder(model.OrderId);
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> FileAttach2(vm_FileAttach model, string InvoiceImg)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                if (UserGroupTypeId == 1)
                {
                    vm_jsOutput order = await objSettings.VerifyOrderActive(model.OrderId);
                    if (order != null && order.StatusId == -2)
                    {
                        return Json(output);
                    }
                }
                if (string.IsNullOrEmpty(InvoiceImg))
                {
                    output.Message = Translation.UploadAttachment;
                    return Json(output);
                }
                vm_OrderStatus attach = new vm_OrderStatus
                {
                    OrderId = model.OrderId,
                    InvoiceImage = InvoiceImg,
                    Comments = model.Comments,
                    Status = 9
                };
                int canUpdater = objSettings.CanUpdateAttachment(attach);
                if (canUpdater == 1)
                {
                    attach.Status = 9;
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await objSettings.UpdateAttachment(attach);
                }
                else
                {
                    attach.Status = 9;
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await objSettings.UpdateHistory(attach);
                }
                if (output.StatusId == 1)
                {
                    await cls_Sms.CompleteOrder(model.OrderId);
                    await cls_Email.FinishOrder(model.OrderId);
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return Json(output);
        }

        public ActionResult AdditionalService(string Id)
        {
            vm_FileAttach model = new vm_FileAttach
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };
            base.ViewBag.Vat = objSettings.Vat();
            return PartialView("_AdditionalService", model);
        }

        [HttpPost]
        public async Task<JsonResult> AdditionalService(FormCollection data)
        {
            vm_jsOutput output = new vm_jsOutput();
            HttpFileCollectionBase files = base.Request.Files;
            HttpPostedFileBase invoice = null;
            string invoiceImage = "";
            try
            {
                if (base.Request.Files.Count > 0)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string strServices = data["model"].ToString();
                    List<vm_AdditionalService> model = serializer.Deserialize<List<vm_AdditionalService>>(strServices);
                    if (files["CustomerSignOff"] == null)
                    {
                        output.Message = Translation.UploadSignOff;
                        return Json(output);
                    }
                    HttpPostedFileBase signoff = files["CustomerSignOff"];
                    string customerSignOff = Path.GetExtension(signoff.FileName);
                    if (model.Count > 0)
                    {
                        if (files["InvoiceImage"] == null)
                        {
                            output.Message = Translation.UploadAttachment;
                            return Json(output);
                        }
                        invoice = files["InvoiceImage"];
                        invoiceImage = Path.GetExtension(invoice.FileName);
                    }
                    vm_OrderStatus attach = new vm_OrderStatus
                    {
                        OrderId = model[0].OrderId,
                        Comments = data[0].ToString(),
                        Status = 9,
                        CustomerSignOff = model[0].OrderId + customerSignOff,
                        InvoiceImage = ((!string.IsNullOrEmpty(invoiceImage)) ? (model[0].OrderId + "At" + invoiceImage) : "")
                    };
                    int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                    if (UserGroupTypeId == 1)
                    {
                        vm_jsOutput order = await objSettings.VerifyOrderActive(attach.OrderId);
                        if (order != null && order.StatusId == -2)
                        {
                            return Json(output);
                        }
                    }
                    vm_Settings setting = await objSettings.GetSetting();
                    vm_jsOutput vm_jsOutput = output;
                    vm_jsOutput.StatusId = await objSettings.FinishOrders(model, attach, Convert.ToDecimal(setting.Vat));
                    if (output.StatusId > 0)
                    {
                        string strPath = cls_Defaults.InvoiceUploadPath + attach.OrderId + "/";
                        if (!Directory.Exists(strPath))
                        {
                            Directory.CreateDirectory(strPath);
                        }
                        if (!string.IsNullOrEmpty(customerSignOff))
                        {
                            if (System.IO.File.Exists(strPath + attach.CustomerSignOff))
                            {
                                System.IO.File.Delete(strPath + attach.CustomerSignOff);
                            }
                            signoff.SaveAs(strPath + attach.CustomerSignOff);
                        }
                        if (!string.IsNullOrEmpty(invoiceImage))
                        {
                            if (System.IO.File.Exists(strPath + attach.InvoiceImage))
                            {
                                System.IO.File.Delete(strPath + attach.InvoiceImage);
                            }
                            invoice.SaveAs(strPath + attach.InvoiceImage);
                        }
                    }
                    if (output.StatusId > 0)
                    {
                        await cls_Sms.CompleteOrder(attach.OrderId);
                        await cls_Email.FinishOrder(attach.OrderId);
                    }
                }
                else
                {
                    output.Message = Translation.UploadSignOff;
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public ActionResult GetOrderAdditionalWork(string Id)
        {
            int OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
            List<vm_OrderAdditionalWork> model = objSettings.GetAdditionalWorkByOrderId(OrderId);
            return PartialView("_GetOrderAdditionalWork", model);
        }

        public ActionResult VerifyCode(string Id)
        {
            int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);

            vm_OrderConfirmCode model = new vm_OrderConfirmCode
            {
                OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id))
            };
            List<SelectListItem> Mlabours = (from s in objUser.GetAssignedLabours(userGroupTypeId4, Convert.ToDateTime(DateTime.Now), 0, model.OrderId)
                                             select new SelectListItem
                                             {
                                                 Text = s.FirstName + " " + s.LastName,
                                                 Value = s.UserId.ToString()
                                             }).ToList();
            base.ViewBag.Labours = Mlabours;
            return PartialView("_verifyCode", model);
        }

        [HttpPost]
        public async Task<JsonResult> VerifyCode(vm_OrderConfirmCode model, int[] LabourId)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                string strcode = (await objSettings.GetOrderById(model.OrderId)).CustomerCode.ToString();
                if (strcode.Equals(model.Code))
                {
                    output.StatusId = 1;
                    int userGroupTypeId4 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                    vm_OrderStatus newmodel = new vm_OrderStatus();
                    newmodel.OrderId = model.OrderId;
                    newmodel.Status = (int)OrderStatus.Finish;
                    output.StatusId = await objSettings.EditBulkOrderStatus(newmodel, LabourId, (int)OrderStatus.Finish);
                    output.Message = Translation.Finish;

                }
                else
                {
                    output.StatusId = -1;
                    output.Message = Translation.InvalidCode;
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public JsonResult GetAdditionalServices(int cat)
        {
            vm_Result objResult = new vm_Result();
            List<vm_AdditionalWork> result = (List<vm_AdditionalWork>)(objResult.Data = cls_DropDowns.GetAdditionalWorksCat(cat));
            objResult.Count = result.Count;
            return Json(new
            {
                result = objResult.Data,
                iTotalRecords = objResult.Count,
                iTotalDisplayRecords = objResult.Count
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Invoice()
        {
            return View();
        }

        [SiteAuthorize(new string[] { "Provider", "Admin", "SuperAdmin" })]
        public ActionResult Complain()
        {
            base.ViewBag.Title = Translation.ComplainList;
            base.ViewBag.IsNew = 1;
            return View();
        }

        [SiteAuthorize(new string[] { "Provider", "Admin", "SuperAdmin" })]
        public ActionResult Resolved()
        {
            base.ViewBag.Title = Translation.ArchieveComplain;
            base.ViewBag.IsNew = 0;
            return View("Complain");
        }

        public async Task<JsonResult> GetItems(vm_JqueryDataTables model)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<Item> filters = new Filters<Item>();
                Sorts<Item> sorts = new Sorts<Item>();
                sorts.Add(model.iSortCol_0 == 0, (Item x) => x.Id, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (Item x) => x.name, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (Item x) => x.name_en, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<Item> result = objSettings.GetItem(model.iDisplayLength, PageNo, sorts, filters, userGroupId);
                List<Item> lst = result.Results.ToList();
                List<vm_Item> output = (List<vm_Item>)(objResult.Data = Mapper.Map<List<Item>, List<vm_Item>>(lst));
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

        public async Task<JsonResult> GetComplain(vm_JqueryDataTables model, int isNew)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrderComplain> filters = new Filters<tblOrderComplain>();
                Sorts<tblOrderComplain> sorts = new Sorts<tblOrderComplain>();
                if (userGroupTypeId == 1)
                {
                    filters.Add(condition: true, (tblOrderComplain x) => x.tblOrder.ReservedProvider == userGroupId);
                }
                if (isNew == 1)
                {
                    filters.Add(condition: true, (tblOrderComplain x) => x.StatusId != 5 && x.StatusId != 6 && x.StatusId != 7);
                }
                else
                {
                    filters.Add(condition: true, (tblOrderComplain x) => x.StatusId == 6 || x.StatusId == 7);
                }
                sorts.Add(model.iSortCol_0 == 0, (tblOrderComplain x) => x.AddedOn, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrderComplain x) => x.OrderId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 3, (tblOrderComplain x) => x.StatusId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrderComplain> result = objSettings.GetComplains(model.iDisplayLength, PageNo, sorts, filters, isNew);
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
                    int provider = item.ProviderId;
                    vm_GroupCompanies groups = await objUser.GetGroupById(provider);
                    item.Provider = (IsEnglish ? groups.CompanyNameEN : groups.CompanyNameAR);
                    item.Category = (IsEnglish ? item.CategoryEN : item.CategoryAR);
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

        [SiteAuthorize("Provider", "Admin", "SuperAdmin")]
        public async Task<ActionResult> ComplainDetail(string Id)
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

                    var status = model.Status;
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

                    return View(model);
                }
                else
                    return RedirectToAction("Complain");
            }
            catch (Exception ex) { }
            return HttpNotFound();
        }

        public ActionResult AddItem()
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
        public async Task<JsonResult> AddItem(vm_Item model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.AddItem(model, userGroupId);
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
                if ((model.StatusId == 4 || model.StatusId == 6) && string.IsNullOrEmpty(model.Response))
                {
                    output.Message = "Enter your comments.";
                    return Json(output);
                }
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.UpdateComplain(model);
            }
            catch (Exception)
            {
            }
            return Json(output);
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
                int UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Page<tblAdminUser> result = objUser.GetServiceProviderUsers(model.iDisplayLength, PageNo, filters, sorts, UserGroupID);
                List<tblAdminUser> lst = result.Results.ToList();
                List<vm_UserList> output = Mapper.Map<List<tblAdminUser>, List<vm_UserList>>(lst);
                foreach (vm_UserList item in output)
                {
                    item.CompanyName = (IsEnglish ? item.CompanyNameEN : item.CompanyNameAR);
                    vm_UserList vm_UserList = item;
                    vm_UserList.AccountName = await objUser.GetAccountName(item.AccountTypeId);
                    if ((from l in lst
                         where l.UserId == item.UserId
                         select l.LabourIsDriver).FirstOrDefault())
                    {
                        item.AccountName = Translation.LabourandDriver;
                    }
                    if (item.IsLogin && (item.AccountTypeId == 10 || item.AccountTypeId == 11))
                    {
                        string Status = "<p style='color:Green'>" + item.AccountName + "</p>";
                        item.AccountName = Status;
                    }
                    else if (!item.IsLogin && (item.AccountTypeId == 10 || item.AccountTypeId == 11))
                    {
                        string Status = "<p style='color:Red'>" + item.AccountName + "</p>";
                        item.AccountName = Status;
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

        [SiteAuthorize(new string[] { "Provider" })]
        [HttpGet]
        public ActionResult AddUser()
        {
            int userId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            base.ViewBag.UserGroupTypeID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            base.ViewBag.UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            base.ViewBag.UserGroup = objSettings.SelectUserGroup(Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]));
            return View();
        }

        [HttpPost]
        public ActionResult DeleteItem(string Id)
        {
            try
            {
                vm_jsOutput output = new vm_jsOutput();
                Task<int> model = objSettings.DeleteItem(int.Parse(Id));
                output.StatusId = 1;
                output.Message = "Deleted";
                return Json(output);
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [SiteAuthorize(new string[] { "Provider" })]
        [HttpGet]
        public async Task<ActionResult> EditUser(string Id)
        {
            try
            {
                base.ViewBag.UserGroupTypeID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                base.ViewBag.UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                base.ViewBag.UserGroup = objSettings.SelectUserGroup(Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]));
                int UserId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                vm_User model = await objUser.GetUserById(UserId);
                if (model.LaborBlockDate == null)
                {
                    base.ViewBag.BlockDate = null;
                }
                else
                {
                    base.ViewBag.BlockDate = model.LaborBlockDate;
                }
                base.ViewBag.Logo = model.ProfilePic;
                if (model.UserGroupTypeId > 0)
                {
                    base.ViewBag.UserGroup = objUser.GetGroupByType(model.UserGroupTypeId, 0);
                }
                List<SelectListItem> AccountType = objUser.GetAccountType(model.UserGroupTypeId);
                objUser.GetAccountType(0);
                base.ViewBag.AccountType = AccountType;
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                base.ViewBag.UserGroupTypeID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                base.ViewBag.UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                base.ViewBag.UserGroup = objSettings.SelectUserGroup(Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]));
                return View(model);
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [SiteAuthorize(new string[] { "Provider" })]
        [HttpPost]
        public async Task<JsonResult> AddEditUser(vm_User model)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                model.UserGroupTypeId = (byte)base.Session[cls_Defaults.Session_UserGroupTypeId];
                model.UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                db_User objUser = new db_User();
                string strImage = model.ProfilePic;
                string strExt = "";
                if (!string.IsNullOrEmpty(strImage))
                {
                    strExt = (model.ProfilePic = Path.GetExtension(model.ProfilePic));
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
                    if (wokrhrs == null || Convert.ToInt32(wokrhrs.KeyValue) <= 0)
                    {
                        output.Message = Translation.SetWorkingHoursFirst;
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

        public ActionResult GetUserList()
        {
            return View();
        }

        public JsonResult BindAccountType(int Id)
        {
            int typeid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            if (typeid == 1)
            {
                List<SelectListItem> model2 = cls_DropDowns.DDL_ServiceProviderAccountTypes();
                return Json(model2, JsonRequestBehavior.AllowGet);
            }
            List<SelectListItem> model = objUser.GetAccountType(Id);
            List<SelectListItem> Default = objUser.GetAccountType(0);
            IEnumerable<SelectListItem> listFinal = model.Union(Default);
            return Json(listFinal, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindUserGroup(int Id)
        {
            List<SelectListItem> model = objUser.GetGroupByType(Id, 0);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SendNewPasswordSMS(string Id)
        {
            vm_jsOutput output = new vm_jsOutput();
            int UserId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
            tblAdminUser model = objUser.GetUser(UserId);
            string Password;
            if (string.IsNullOrEmpty(model.EncryptedPassword))
            {
                Password = cls_Defaults.GenerateCode(8);
                await objUser.UpdatePassword(model.UserId, Password);
            }
            else
            {
                Password = objSettings.DecryptString(model.EncryptedPassword, useHashing: false);
            }
            if (model != null)
            {
                await cls_Sms.NewUser(model.UserId, model.Email, Password);
                output.StatusId = 1;
            }
            else
            {
                output.StatusId = 0;
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorize(new string[] { "Provider" })]
        public ActionResult ServiceProviderDashboard(vm_Dashboard obj, string submit)
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
        public JsonResult GetProviderDashboardWidgetsData(string StartDate, string EndDate)
        {
            try
            {
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                vm_jsOutput output = new vm_jsOutput();
                vm_Dashboard obj = new vm_Dashboard();
                obj = (vm_Dashboard)(output.Data = objSettings.GetProviderDashboardWidgetsData(StartDate, EndDate, userGroupId, UserId));
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
                obj = (vm_Dashboard)(output.Data = objSettings.GetProviderServiceDistribution(StartDate, EndDate, userGroupId));
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
                obj = (vm_Dashboard)(output.Data = objSettings.GetProviderCompleteOrders(StartDate, EndDate, userGroupId));
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
                obj = (vm_Dashboard)(output.Data = objSettings.GetProviderWorkersList(StartDate, EndDate, userGroupId));
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
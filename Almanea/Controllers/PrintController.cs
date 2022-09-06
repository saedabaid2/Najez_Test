using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
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
using ClosedXML.Excel;
using EntityFrameworkPaginate;
using Microsoft.CSharp.RuntimeBinder;
using Rotativa;

namespace Almanea.Controllers
{

    public class PrintController : BaseController
    {
        private db_Settings objSettings = new db_Settings();

        private db_User objUser = new db_User();

        private AlmaneaDbEntities _context;

        public async Task<ActionResult> OrderDetails(string Id, string culture)
        {
            try
            {
                ViewBag.Vat = objSettings.Vat();
                ViewBag.Id = Id;
                culture = Thread.CurrentThread.CurrentUICulture.Name.ToLowerInvariant();
                int OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));
                vm_Order model = await objSettings.GetOrderById(OrderId);
                if (model != null)
                {

                    ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);
                    var History = await objSettings.GetHistory(OrderId);

                    ViewBag.Additional = await objSettings.GetAdditional(OrderId);
                    if (History.Any((vm_OrderHistoryList x) => x.Status == (byte)OrderStatus.Finish))
                    {
                        vm_OrderHistoryList hist = History.FirstOrDefault((vm_OrderHistoryList x) => x.Status == 9 && !string.IsNullOrEmpty(x.FileAttachment));
                        if (!string.IsNullOrEmpty(hist.FileAttachment))
                        {
                            base.ViewBag.ExistAttachment = hist.FileAttachment;
                        }
                    }
                    culture = cultureHelper.GetImplementedCulture(culture);
                    HttpCookie cookie = base.Request.Cookies["_culture"];
                    if (cookie != null)
                    {
                        cookie.Value = culture;
                    }
                    else
                    {
                        cookie = new HttpCookie("_culture");
                        cookie.Value = culture;
                        cookie.Expires = DateTime.Now.AddYears(1);
                    }
                    base.Response.Cookies.Add(cookie);
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                    base.ViewBag.Culture = culture;
                    base.ViewBag.History = History;
                    return View(model);
                }
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [SiteAuthorize(new string[] { "Admin", "Provider" })]
        public ActionResult Details(string Id)
        {
            string culture = Thread.CurrentThread.CurrentUICulture.Name.ToLowerInvariant();
            return new ActionAsPdf("OrderDetails", new
            {
                Id = Id,
                Culture = culture
            });
        }

        public async Task<FileResult> Installation(string Seller, string Customer, string InvoiceNo, string FromDate, string ToDate, int Location = 0, int StatusId = 0)
        {
            new vm_Result();
            try
            {
                int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Equals(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Equals(Customer) || x.CustomerName.Contains(Customer) || x.CustomerContact.Equals(Customer));
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    DateTime dateTo = DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateFrom = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                else if (!string.IsNullOrEmpty(FromDate))
                {
                    DateTime date2 = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate == date2);
                }
                else if (!string.IsNullOrEmpty(ToDate))
                {
                    DateTime date = DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
                }
                filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == SupplierId && x.Status != 9 && x.Status != 10 && x.Status != 12 && x.Status != 11);
                sorts.Add(condition: true, (tblOrder x) => x.AddedDate, byDescending: true);
                Page<tblOrder> result = objSettings.GetOrders(100000, PageNo, sorts, filters);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderPrint> output = Mapper.Map<List<tblOrder>, List<vm_OrderPrint>>(lst);
                if (userGroupTypeId == 3)
                {
                    db_User objUser = new db_User();
                    foreach (vm_OrderPrint item in output)
                    {
                        if (item.ReservedProvider > 0)
                        {
                            item.ReservedBy = string.Concat(str2: await objUser.GetGroupName(item.ReservedProvider), str0: await objSettings.GetOrderReservedBy(Convert.ToInt32(item.OrderId)), str1: " / ");
                        }
                    }
                }
                DataTable dataTable = output.ToDataTable();
                dataTable.Columns.Remove("ReservedProvider");
                dataTable.Columns.Remove("OrderId");
                if (userGroupTypeId == 2)
                {
                    dataTable.Columns.Remove("ReservedBy");
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dataTable);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Installation.xlsx");
                    }
                }

            }
            catch (Exception)
            {
            }
            return null;
        }

        public async Task<FileResult> Order(string Seller, string Customer, string InvoiceNo, string InstallDate, int TypeId, int? Location = 0, int Status = 0, int Company = 0, int ServiceId = 0, int Direction = 0)
        {
            try
            {
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int AccounType = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int ProviderId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Contains(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Contains(Customer));
                filters.Add(Location > 0, (tblOrder x) => (int?)x.LocationId == Location);
                filters.Add(Direction > 0, (tblOrder x) => x.tblLocation.Direction == (int?)Direction);
                filters.Add(Company > 0, (tblOrder x) => x.ReservedProvider == Company);
                if (ServiceId > 0)
                {
                    filters.Add(condition: true, (tblOrder x) => x.tblOrderServices.Any((tblOrderService y) => y.tblService.ServiceId == ServiceId));
                }
                if (!string.IsNullOrEmpty(InstallDate))
                {
                    DateTime ddt = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(!string.IsNullOrEmpty(InstallDate), (tblOrder x) => x.InstallDate == ddt);
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
                switch (UserGroupTypeId)
                {
                    case 2:
                        if (TypeId == 1)
                        {
                            if (Status > 0)
                            {
                                filters.Add(condition: true, (tblOrder x) => x.Status == Status);
                            }
                            else
                            {
                                filters.Add(condition: true, (tblOrder x) => x.SupplierId == UserGroupId && x.Status != 9 && x.Status != 10 && x.Status != 12 && x.Status != 11);
                            }
                        }
                        else
                        {
                            filters.Add(condition: true, (tblOrder x) => x.SupplierId == UserGroupId && (x.Status == 9 || x.Status == 10 || x.Status == 12));
                        }
                        break;
                    case 3:
                        if (TypeId == 1)
                        {
                            if (Status > 0)
                            {
                                filters.Add(condition: true, (tblOrder x) => x.Status == Status);
                            }
                            else
                            {
                                filters.Add(condition: true, (tblOrder x) => x.Status != 10 && x.Status != 11);
                            }
                        }
                        else if (Status == 12)
                        {
                            filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
                            filters.Add(condition: true, (tblOrder x) => x.Status == 12 || x.Status == 11);
                        }
                        else
                        {
                            filters.Add(condition: true, (tblOrder x) => x.Status == Status);
                        }
                        break;
                    case 1:
                        if (TypeId == 1)
                        {
                            if (Status > 0)
                            {
                                filters.Add(condition: true, (tblOrder x) => x.Status == Status);
                                break;
                            }
                            filters.Add(condition: true, (tblOrder x) => (x.Status == 1 && !x.tblOrderHistories.Any((tblOrderHistory y) => y.OrderId == x.OrderId && y.ServiceProviderId == (int?)UserGroupId && y.Status == 3)) || (x.ReservedProvider == UserGroupId && x.Status != 10 && x.Status != 9 && x.Status != 12 && x.Status != 11));
                        }
                        else if (Status == 9)
                        {
                            filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == UserGroupId && (x.Status == Status || x.Status == 10 || x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == Status)));
                        }
                        else
                        {
                            filters.Add(condition: true, (tblOrder x) => x.Status == Status && x.ReservedProvider == UserGroupId);
                        }
                        break;
                    default:
                        {
                            filters.Add(condition: true, (tblOrder x) => x.SupplierId == UserGroupId && x.Status == 5);
                            if (TypeId <= 0)
                            {
                                break;
                            }
                            DateTime date = DateTime.Today;
                            switch (TypeId)
                            {
                                case 1:
                                    date = DateTime.Today;
                                    break;
                                case 2:
                                    date = DateTime.Today.AddDays(1.0);
                                    break;
                            }
                            if (TypeId == 3)
                            {
                                while (true)
                                {
                                    int dow = (int)date.DayOfWeek;
                                    if (dow == 6)
                                    {
                                        break;
                                    }
                                    date = date.AddDays(1.0);
                                }
                                filters.Add(condition: true, (tblOrder x) => x.InstallDate >= DateTime.Today && x.InstallDate <= date);
                            }
                            else if (TypeId == 4)
                            {
                                while (date.DayOfWeek != 0)
                                {
                                    date = date.AddDays(1.0);
                                }
                                filters.Add(condition: true, (tblOrder x) => x.InstallDate >= date && x.InstallDate <= date.AddDays(7.0));
                            }
                            else
                            {
                                filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
                            }
                            break;
                        }
                }
                sorts.Add(condition: true, (tblOrder x) => x.AddedDate, byDescending: true);
                new List<tblOrder>();
                List<tblOrder> result = ((UserGroupTypeId == 2) ? objSettings.GetOrderssupplier(10000, 1, sorts, filters, UserGroupId, AccounType, UserId).Results.ToList() : ((Status == 12) ? objSettings.GetOrdersOnlySP(10000, 1, sorts, filters, ProviderId, 0).Results.ToList() : objSettings.GetOrdersOnlySP(10000, 1, sorts, filters, ProviderId, 0).Results.Where((tblOrder k) => k.Status != 12).ToList()));
                List<vm_OrderPrint> output = Mapper.Map<List<tblOrder>, List<vm_OrderPrint>>(result);
                foreach (vm_OrderPrint item2 in output)
                {
                    tblProviderTimeSlot timeslot = objSettings.GetTimeslot(int.Parse(item2.OrderNo));
                    if (timeslot != null)
                    {
                        item2.InstallDate = item2.InstallDate.ToString().Substring(0, 10) + " " + cls_Defaults.get12hour(timeslot.StartHour.Value, timeslot.EndHour.Value);
                    }
                    else
                    {
                        item2.InstallDate = item2.InstallDate.ToString().Substring(0, 10);
                    }
                }
                if (UserGroupTypeId == 3)
                {
                    db_User objUser = new db_User();
                    foreach (vm_OrderPrint item in output)
                    {
                        if (item.ReservedProvider > 0)
                        {
                            item.ReservedBy = string.Concat(str2: await objUser.GetGroupName(item.ReservedProvider), str0: await objSettings.GetOrderReservedBy(item.OrderId), str1: " / ");
                        }
                    }
                }
                DataTable dataTable = output.ToDataTable();
                dataTable.Columns.Remove("ReservedProvider");
                dataTable.Columns.Remove("OrderId");
                if (UserGroupTypeId == 2)
                {
                    dataTable.Columns.Remove("ReservedBy");
                }
                dataTable.Columns.Remove("Unit");

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dataTable);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public FileResult GetSalesReport(int id, string sDate, string EDate)
        {
            try
            {
                vm_Result objResult = new vm_Result();
                bool isEnglish = cls_Defaults.IsEnglish;
                int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                DateTime datefrom = Convert.ToDateTime(sDate);
                DateTime dateto = Convert.ToDateTime(EDate);
                TimeSpan ts = new TimeSpan(23, 59, 59);
                TimeSpan ts2 = new TimeSpan(0, 0, 0);
                datefrom = datefrom.Add(ts2);
                dateto = dateto.Add(ts);
                IEnumerable<tblOrder> result = objSettings.GetSalesReportPrint(id, SupplierId, datefrom, dateto);
                if (id > 0)
                {
                    result = result.Where((tblOrder p) => p.AddedBy == id);
                }
                List<tblOrder> lst = result.ToList();
                List<vm_SalesOrderPrint> output = Mapper.Map<List<tblOrder>, List<vm_SalesOrderPrint>>(lst);
                string x = datefrom.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                foreach (vm_SalesOrderPrint item in output)
                {
                    item.Quantity = objSettings.getorderquantity(item.OrderId);
                }
                DataTable dataTable = output.ToDataTable();
                dataTable.Columns.Remove("OrderId");
                dataTable.Columns.Remove("OrderNo");
                string filename = "Syanah_Orders_" + DateTime.Now.ToString() + ".xlsx";
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dataTable, "Syanah_Orders");
                    base.Response.Clear();
                    base.Response.Buffer = true;
                    base.Response.Charset = "";
                    base.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    base.Response.AddHeader("content-disposition", "attachment;filename=" + filename);
                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(base.Response.OutputStream);
                        base.Response.Flush();
                        base.Response.End();
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public JsonResult GetGroupName(int Id)
        {
            List<SelectListItem> output = objUser.GetGroupByType(Id, 0);
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorize(new string[] { "Admin", "SuperAdmin", "Supplier" })]
        public ActionResult Invoice(int TypeId = 1)
        {
            if (TypeId != 1)
            {
                base.ViewBag.TypeId = base.Url.Action("Report", "Print");
            }
            else
            {
                base.ViewBag.TypeId = base.Url.Action("DetailReport", "Print");
            }
            return View();
        }

        [SiteAuthorize(new string[] { "Supplier" })]
        public ActionResult SupplierInvoice(int TypeId = 1)
        {
            base.ViewBag.TypeId = base.Url.Action("DetailReportSupplier", "Print");
            return View();
        }

        public async Task<ActionResult> ReportInvoice(int UserGroupTypeId, int UserGroupId, string fromDate, string toDate)
        {
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                switch (UserGroupTypeId)
                {
                    case 2:
                        filters.Add(UserGroupId > 0, (tblOrder x) => x.SupplierId == UserGroupId);
                        break;
                    case 1:
                        filters.Add(UserGroupId > 0, (tblOrder x) => x.ReservedProvider == UserGroupId);
                        break;
                }
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.InstallDate, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                vm_GroupCompanies dtUserGroup = await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(10000, 1, sorts, filters);
                base.ViewBag.Vat = dtSetting.Vat;
                decimal ContractPercent = Convert.ToDecimal(dtSetting.ContractPercent);
                base.ViewBag.ContractPercent = ContractPercent;
                decimal SubTotal = default(decimal);
                decimal Vat = default(decimal);
                List<vm_Invoice_OrderDate> output = new List<vm_Invoice_OrderDate>();
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    IEnumerable<DateTime> dates = result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (DateTime d in dates)
                    {
                        vm_Invoice_OrderDate item = new vm_Invoice_OrderDate
                        {
                            Date = d.ToString("dd/MM/yyy")
                        };
                        List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
                        foreach (tblOrder i in from x in result
                                               where x.AddedDate == d
                                               orderby x.OrderId
                                               select x)
                        {
                            List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                            orders.Add(new vm_Invoice_Orders
                            {
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameAR,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                SubTotal = i.TotalAmount,
                                Services = services
                            });
                            Vat += i.ServiceVat.Value + (i.AdditionalVat.HasValue ? i.AdditionalVat.Value : 0m);
                            SubTotal += i.ServiceTotal.Value - i.AdditionalTotal.Value;
                        }
                        item.Orders = orders;
                        output.Add(item);
                    }
                }
                vm_Invoice model = new vm_Invoice
                {
                    Company_Title = dtSetting.CompanyName,
                    Company_Email = dtSetting.CompanyEmail,
                    Company_Phone = dtSetting.CompanyPhone,
                    Company_Website = dtSetting.CompanyWebsite,
                    InvoiceDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    DueDate = toDate,
                    InvoiceNo = cls_Defaults.GenerateUniqueId(),
                    UserGroup = dtUserGroup.CompanyNameAR,
                    TotalOrders = dtOrders.RecordCount,
                    Orders = output,
                    PeriodTime = $"{fromDate} - {toDate}",
                    SubTotal = SubTotal,
                    Vat = Vat,
                    Total = SubTotal + Vat,
                    Due = SubTotal * (ContractPercent / 100m)
                };
                return View(model);
            }
            catch (Exception)
            {
            }
            return View();
        }

        public ActionResult Report(int UserGroupTypeId, int UserGroupId, string fromDate, string toDate)
        {
            return new ActionAsPdf("ReportInvoice", new { UserGroupTypeId, UserGroupId, fromDate, toDate });
        }

        public ActionResult DetailReport(int userGroupTypeId, int userGroupId, string fromDate, string toDate, int supplierid = 0)
        {
            int userGroupTypeId2 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            int GroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            if (GroupTypeId == 3)
            {
                return new ActionAsPdf("ProviderAdmin", new
                {
                    UserGroupId = userGroupId,
                    fromDate = fromDate,
                    toDate = toDate,
                    supplierid = supplierid
                });
            }
            if (userGroupTypeId2 == 2)
            {
                return new ActionAsPdf("Supplier", new
                {
                    UserGroupId = userGroupId,
                    fromDate = fromDate,
                    toDate = toDate
                });
            }
            return new ActionAsPdf("Provider", new
            {
                UserGroupId = userGroupId,
                fromDate = fromDate,
                toDate = toDate,
                supplierid = supplierid
            });
        }

        public ActionResult DetailReportSupplier(string fromDate, string toDate)
        {
            int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            return new ActionAsPdf("Supplier", new
            {
                UserGroupId = userGroupId,
                fromDate = fromDate,
                toDate = toDate
            });
        }

        public async Task<FileResult> SupplierInvoiceExcel(string fromDate, string toDate)
        {
            Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == UserGroupId);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.OrderId, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(10000, 1, sorts, filters);
                base.ViewBag.Vat = dtSetting.Vat;
                base.ViewBag.ContractPercent = dtSetting.ContractPercent;
                decimal SubTotal = default(decimal);
                decimal Vat = default(decimal);
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (tblOrder i in result.OrderByDescending((tblOrder x) => x.OrderId))
                    {
                        List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                        foreach (vm_Invoice_Services item in services)
                        {
                            orders.Add(new vm_Invoice_Orders
                            {
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameEN,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                ServicesName = item.Title,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                Amount = (decimal)item.Quantity * item.UnitPrice,
                                Vat = item.Vat,
                                SubTotal = (decimal)item.Quantity * item.UnitPrice + item.Vat,
                                AdditonalAmount = i.AdditionalTotal.Value,
                                AdditionalVat = i.AdditionalVat.Value,
                                Services = services
                            });
                        }
                        Vat += i.ServiceVat.Value;
                        SubTotal += i.ServiceTotal.Value;
                    }
                }
            }
            catch (Exception)
            {
            }
            List<vm_Invoice_Orders_Print> output = Mapper.Map<List<vm_Invoice_Orders>, List<vm_Invoice_Orders_Print>>(orders);
            DataTable dataTable = output.ToDataTable();
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FinancialReport.xlsx");
                }
            }
        }

        public async Task<ActionResult> getSupplier(string fromDate, string toDate)
        {
            Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            return View(await SupplierObj(fromDate, toDate, UserGroupId));
        }

        public async Task<JsonResult> SerachSupplierInvoice(vm_JqueryDataTables model, string fromDate, string toDate)
        {
            Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            vm_Result objResult = new vm_Result();
            List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == UserGroupId);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.OrderId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                vm_Settings dtSetting = await objSettings.GetSetting();
                await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);
                base.ViewBag.Vat = dtSetting.Vat;
                base.ViewBag.ContractPercent = dtSetting.ContractPercent;
                decimal SubTotal = default(decimal);
                decimal Vat = default(decimal);
                IEnumerable<tblOrder> result = dtOrders.Results;
                if (dtOrders.RecordCount > 0)
                {
                    result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (tblOrder i in result)
                    {
                        List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                        foreach (vm_Invoice_Services item in services)
                        {
                            orders.Add(new vm_Invoice_Orders
                            {
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameEN,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                ServicesName = item.Title,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                Amount = (decimal)item.Quantity * item.UnitPrice,
                                Vat = item.Vat,
                                SubTotal = (decimal)item.Quantity * item.UnitPrice,
                                Services = services
                            });
                        }
                        Vat += i.ServiceVat.Value;
                        SubTotal += i.ServiceTotal.Value;
                    }
                }
                objResult.Data = orders;
                objResult.Count = dtOrders.RecordCount;
            }
            catch (Exception)
            {
            }
            return Json(new
            {
                aaData = orders,
                sEcho = model.sEcho,
                iTotalRecords = objResult.Count,
                iTotalDisplayRecords = objResult.Count
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Supplier(string fromDate, string toDate, int UserGroupId = 0)
        {
            return View(await SupplierObj(fromDate, toDate, UserGroupId));
        }

        public async Task<FileResult> InvoiceExcelExport(vm_JqueryDataTables model, string fromDate, string toDate, string userGroupTypeId, string userGroupId, int supplierid = 0)
        {
            new vm_Result();
            DataTable dataTable = new DataTable();
            try
            {
                model.iDisplayLength = 0;
                if (userGroupTypeId == 2.ToString())
                {
                    List<vm_Invoice_Orders_Print> output2 = Mapper.Map<List<vm_Invoice_Orders>, List<vm_Invoice_Orders_Print>>((List<vm_Invoice_Orders>)(await SupplierExcel(model, fromDate, toDate, Convert.ToInt32(string.IsNullOrEmpty(userGroupId) ? "0" : userGroupId))).Data);
                    dataTable = output2.ToDataTable();
                }
                else
                {
                    List<vm_Invoice_Orders_Service_Print> output = Mapper.Map<List<vm_Invoice_Orders>, List<vm_Invoice_Orders_Service_Print>>((List<vm_Invoice_Orders>)(await ProviderExcel(model, fromDate, toDate, Convert.ToInt32(string.IsNullOrEmpty(userGroupId) ? "0" : userGroupId), supplierid)).Data);
                    dataTable = output.ToDataTable();
                }
            }
            catch (Exception)
            {
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FinancialReport.xlsx");
                }
            }
        }

        public async Task<JsonResult> DetailReportExcel(vm_JqueryDataTables model, string fromDate, string toDate, string userGroupTypeId, string userGroupId, string supplierid)
        {
            vm_Result data = new vm_Result();
            try
            {
                data = ((!(userGroupTypeId == 2.ToString())) ? (await ProviderExcel(model, fromDate, toDate, Convert.ToInt32(string.IsNullOrEmpty(userGroupId) ? "0" : userGroupId), Convert.ToInt32(string.IsNullOrEmpty(supplierid) ? "0" : supplierid))) : (await SupplierExcel(model, fromDate, toDate, Convert.ToInt32(string.IsNullOrEmpty(userGroupId) ? "0" : userGroupId))));
            }
            catch (Exception)
            {
            }
            return Json(new
            {
                aaData = data.Data,
                sEcho = model.sEcho,
                iTotalRecords = data.Count,
                iTotalDisplayRecords = data.Count
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<vm_Result> SupplierExcel(vm_JqueryDataTables jdt, string fromDate, string toDate, int UserGroupId = 0)
        {
            int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
            int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
            int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
            vm_Result modelResult = new vm_Result();
            new vm_Invoice();
            List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
            int pageSize = 100000;
            int PageNo = 1;
            if (jdt.iDisplayLength > 0)
            {
                pageSize = jdt.iDisplayLength;
                if (jdt.iDisplayStart >= jdt.iDisplayLength)
                {
                    PageNo = jdt.iDisplayStart / jdt.iDisplayLength + 1;
                }
            }
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == UserGroupId);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.OrderId, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                vm_GroupCompanies dtUserGroup = await objUser.GetGroupById(UserGroupId);
                new Page<tblOrder>();
                Page<tblOrder> dtOrders = ((userGroupTypeId != 2 || accountype != 17) ? objSettings.GetOrders(pageSize, PageNo, sorts, filters) : objSettings.GetSupplierAdminOrders(pageSize, PageNo, sorts, filters, userid));
                base.ViewBag.Vat = dtSetting.Vat;
                base.ViewBag.ContractPercent = dtSetting.ContractPercent;
                decimal SubTotal = default(decimal);
                decimal Vat = default(decimal);
                List<vm_Invoice_OrderDate> output = new List<vm_Invoice_OrderDate>();
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    foreach (tblOrder i in result)
                    {
                        List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                        int count = 1;
                        foreach (vm_Invoice_Services ser in services)
                        {
                            vm_Invoice_Orders odr = new vm_Invoice_Orders
                            {
                                AdditionalWorkPrice = ser.AdditionalWork,
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameEN,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                SubTotal = i.TotalAmount - i.AdditionalTotal.Value + ser.Vat + i.AdditionalVat.Value,
                                AdditonalAmount = i.AdditionalTotal.Value,
                                Quantity = ser.Quantity,
                                Amount = ser.Total,
                                Vat = ser.Vat,
                                AdditionalVat = i.AdditionalVat.Value,
                                Services = services,
                                ServicesName = ser.Title,
                                UnitPrice = ser.UnitPrice,
                                Services_UnitPrice = ser.UnitPrice
                            };
                            if (count == 1)
                            {
                                odr.SubTotal = ((services.Count > 1) ? ser.Total : i.TotalAmount);
                                odr.SubTotal = ser.Total + i.ServiceVat.Value + ser.AdditionalVat;
                                odr.AdditonalAmount = i.AdditionalTotal.Value;
                            }
                            else
                            {
                                odr.SubTotal = ser.Total + i.ServiceVat.Value + ser.AdditionalVat;
                            }
                            orders.Add(odr);
                            count++;
                        }
                        Vat += i.ServiceVat.Value;
                        SubTotal += i.ServiceTotal.Value;
                    }
                }
                base.ViewBag.Provider = "";
                new vm_Invoice
                {
                    Company_Title = dtSetting.CompanyName,
                    Company_Email = dtSetting.CompanyEmail,
                    Company_Phone = dtSetting.CompanyPhone,
                    Company_Website = dtSetting.CompanyWebsite,
                    InvoiceDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    DueDate = toDate,
                    InvoiceNo = cls_Defaults.GenerateUniqueId(),
                    UserGroup = dtUserGroup.CompanyNameEN,
                    TotalOrders = dtOrders.RecordCount,
                    Orders = output,
                    PeriodTime = $"{fromDate} - {toDate}",
                    SubTotal = SubTotal,
                    Vat = Vat,
                    Total = SubTotal + Vat,
                    Due = (SubTotal + Vat) * (Convert.ToDecimal(dtSetting.ContractPercent) / 100m)
                };
                modelResult.Count = dtOrders.RecordCount;
                modelResult.Data = orders;
                return modelResult;
            }
            catch (Exception)
            {
                return modelResult;
            }
        }

        public async Task<vm_Result> ProviderExcel(vm_JqueryDataTables jdt, string fromDate, string toDate, int UserGroupId, int supplierid = 0)
        {
            new vm_Invoice();
            vm_Result modelResult = new vm_Result();
            List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
            int pageSize = 100000;
            int PageNo = 1;
            if (jdt.iDisplayLength > 0)
            {
                pageSize = jdt.iDisplayLength;
                if (jdt.iDisplayStart >= jdt.iDisplayLength)
                {
                    PageNo = jdt.iDisplayStart / jdt.iDisplayLength + 1;
                }
            }
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == UserGroupId);
                filters.Add(supplierid > 0, (tblOrder x) => x.SupplierId == supplierid);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.OrderId, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                vm_GroupCompanies dtUserGroup = await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(pageSize, PageNo, sorts, filters);
                dtOrders.Results.Select((tblOrder x) => x.OrderId).ToList();
                decimal VatPercent = Convert.ToDecimal(dtSetting.Vat);
                decimal ContractPercent = Convert.ToDecimal(dtSetting.ContractPercent);
                base.ViewBag.Vat = VatPercent;
                base.ViewBag.ContractPercent = ContractPercent;
                decimal ServiceAmount = default(decimal);
                decimal ServiceVat = default(decimal);
                decimal AdditionalAmount = default(decimal);
                decimal AdditionalVat = default(decimal);
                decimal FinalAmount = default(decimal);
                List<vm_Invoice_OrderDate> output = new List<vm_Invoice_OrderDate>();
                List<int> serviceCount = new List<int>();
                Dictionary<string, int> serviceCountList = new Dictionary<string, int>();
                Dictionary<string, decimal> serviceAmountList = new Dictionary<string, decimal>();
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (tblOrder i in result)
                    {
                        List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                        int count = 1;
                        foreach (vm_Invoice_Services item in services)
                        {
                            _ = item.Total + item.Vat;
                            vm_Invoice_Orders odr = new vm_Invoice_Orders
                            {
                                AdditionalWorkPrice = item.AdditionalWork,
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameEN,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                Quantity = item.Quantity,
                                Amount = (decimal)item.Quantity * item.UnitPrice,
                                Vat = i.ServiceVat.Value,
                                UnitPrice = item.UnitPrice,
                                AdditionalVat = decimal.Parse(item.AdditionalVat.ToString("0.00")),
                                AdditonalAmount = decimal.Parse(item.AdditonalAmount.ToString("0.00")),
                                ServicesName = item.Title,
                                Services_Quantity = item.Quantity,
                                Services_Title = item.Title,
                                Services_Total = item.Total,
                                Services_UnitPrice = item.UnitPrice,
                                Services_Vat = item.Vat,
                                Services = services
                            };
                            if (count == 1)
                            {
                                odr.SubTotal = item.AdditonalAmount + (decimal)item.Quantity * item.UnitPrice + i.ServiceVat.Value + item.AdditionalVat;
                            }
                            else
                            {
                                odr.SubTotal = item.AdditonalAmount + (decimal)item.Quantity * item.UnitPrice + i.ServiceVat.Value + item.AdditionalVat;
                            }
                            orders.Add(odr);
                            count++;
                        }
                        ServiceAmount += i.ServiceTotal.Value;
                        ServiceVat += i.ServiceVat.Value;
                        FinalAmount += i.TotalAmount;
                        serviceCount.Add(objSettings.getorderquantity(i.OrderId));
                        serviceCountList.Add(i.OrderId.ToString(), objSettings.getorderquantity(i.OrderId));
                        serviceAmountList.Add(i.OrderId.ToString(), i.ServiceTotal.Value + i.ServiceVat.Value);
                    }
                }
                if (orders.Count > 0)
                {
                    (from k in orders
                     group k by k.OrderNo into g
                     where g.Count() > 1
                     select g into y
                     select y.Key).ToList();
                    List<vm_Invoice_Orders> collection = (from k in orders
                                                          group k by k.OrderNo).SelectMany((IGrouping<string, vm_Invoice_Orders> k) => k).ToList();
                    _ = collection.ToList().Count;
                    _ = 1;
                }
                vm_Invoice model = new vm_Invoice
                {
                    Company_Title = dtSetting.CompanyName,
                    Company_Email = dtSetting.CompanyEmail,
                    Company_Phone = dtSetting.CompanyPhone,
                    Company_Website = dtSetting.CompanyWebsite,
                    InvoiceDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    DueDate = toDate,
                    InvoiceNo = cls_Defaults.GenerateUniqueId(),
                    TotalOrders = dtOrders.RecordCount,
                    Orders = output,
                    PeriodTime = $"{fromDate} - {toDate}",
                    SubTotal = ServiceAmount + AdditionalAmount,
                    Vat = ServiceVat + AdditionalVat,
                    Total = FinalAmount,
                    Additional = AdditionalAmount + AdditionalVat,
                    ServiceAmount = ServiceVat + ServiceAmount
                };
                if (UserGroupId > 0)
                {
                    model.UserGroup = dtUserGroup.CompanyNameEN;
                }
                decimal contract = ContractPercent / 100m;
                decimal due_Service = ServiceAmount - ServiceAmount * contract;
                decimal due_additional = AdditionalAmount - AdditionalAmount * contract;
                decimal due_vat_Service = due_Service + due_Service * (VatPercent / 100m);
                decimal due_vat_Additonal = due_additional + due_additional * (VatPercent / 100m);
                decimal total_orderAmount = due_vat_Additonal + due_vat_Service;
                decimal pay_Sp = total_orderAmount - model.Additional;
                decimal due = (model.Due = model.ServiceAmount - pay_Sp);
                model.PayToSp = pay_Sp;
                modelResult.Count = dtOrders.RecordCount;
                modelResult.Data = orders;
                return modelResult;
            }
            catch (Exception)
            {
                return modelResult;
            }
        }

        public async Task<vm_Invoice> SupplierObj(string fromDate, string toDate, int UserGroupId = 0)
        {
            vm_Invoice model = new vm_Invoice();
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == UserGroupId);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.OrderId, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                vm_GroupCompanies dtUserGroup = await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(10000, 1, sorts, filters);
                base.ViewBag.Vat = dtSetting.Vat;
                base.ViewBag.ContractPercent = dtSetting.ContractPercent;
                decimal SubTotal = default(decimal);
                decimal Vat = default(decimal);
                List<vm_Invoice_OrderDate> output = new List<vm_Invoice_OrderDate>();
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    IEnumerable<DateTime> dates = result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (DateTime d in dates)
                    {
                        vm_Invoice_OrderDate item = new vm_Invoice_OrderDate
                        {
                            Date = d.ToString("dd/MM/yyy")
                        };
                        List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
                        foreach (tblOrder i in from x in result
                                               where x.AddedDate == d
                                               orderby x.OrderId
                                               select x)
                        {
                            List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                            orders.Add(new vm_Invoice_Orders
                            {
                                AdditionalWorkPrice = services[0].AdditionalWork,
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameEN,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                SubTotal = i.TotalAmount - i.AdditionalTotal.Value,
                                AdditonalAmount = i.AdditionalTotal.Value,
                                Quantity = objSettings.getorderquantity(i.OrderId),
                                Amount = i.ServiceTotal.Value,
                                Vat = i.ServiceVat.Value,
                                AdditionalVat = i.AdditionalVat.Value,
                                Services = services
                            });
                            foreach (vm_Invoice_Services sv in services)
                            {
                                Vat += sv.Vat;
                                SubTotal += sv.Total;
                            }
                        }
                        item.Orders = orders;
                        output.Add(item);
                    }
                }
                base.ViewBag.Provider = "";
                model = new vm_Invoice
                {
                    Company_Title = dtSetting.CompanyName,
                    Company_Email = dtSetting.CompanyEmail,
                    Company_Phone = dtSetting.CompanyPhone,
                    Company_Website = dtSetting.CompanyWebsite,
                    InvoiceDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    DueDate = toDate,
                    InvoiceNo = cls_Defaults.GenerateUniqueId(),
                    UserGroup = dtUserGroup.CompanyNameEN,
                    TotalOrders = dtOrders.RecordCount,
                    Orders = output,
                    PeriodTime = $"{fromDate} - {toDate}",
                    SubTotal = SubTotal,
                    Vat = Vat,
                    Total = SubTotal + Vat,
                    Due = (SubTotal + Vat) * (Convert.ToDecimal(dtSetting.ContractPercent) / 100m)
                };
                return model;
            }
            catch (Exception)
            {
                return model;
            }
        }

        public async Task<ActionResult> Provider(string fromDate, string toDate, int UserGroupId, int supplierid = 0, string username = "")
        {
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == UserGroupId);
                filters.Add(supplierid > 0, (tblOrder x) => x.SupplierId == supplierid);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.OrderId, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                vm_GroupCompanies dtUserGroup = await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(10000, 1, sorts, filters);
                dtOrders.Results.Select((tblOrder x) => x.OrderId).ToList();
                decimal VatPercent = Convert.ToDecimal(dtSetting.Vat);
                decimal ContractPercent = Convert.ToDecimal(dtSetting.ContractPercent);
                base.ViewBag.Vat = VatPercent;
                base.ViewBag.ContractPercent = ContractPercent;
                decimal ServiceAmount = default(decimal);
                decimal ServiceVat = default(decimal);
                decimal AdditionalAmount = default(decimal);
                decimal AdditionalVat = default(decimal);
                List<vm_Invoice_OrderDate> output = new List<vm_Invoice_OrderDate>();
                new List<int>();
                new Dictionary<string, int>();
                new Dictionary<string, decimal>();
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    IEnumerable<DateTime> dates = result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (DateTime d in dates)
                    {
                        vm_Invoice_OrderDate item = new vm_Invoice_OrderDate
                        {
                            Date = d.ToString("dd/MM/yyy")
                        };
                        List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
                        foreach (tblOrder i in from x in result
                                               where x.AddedDate == d
                                               orderby x.OrderId
                                               select x)
                        {
                            List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                            int count = 0;
                            foreach (vm_Invoice_Services item2 in services)
                            {
                                _ = item2.Total + item2.Vat;
                                vm_Invoice_Orders odr = new vm_Invoice_Orders
                                {
                                    OrderNo = i.OrderId.ToString(),
                                    InvoiceNo = i.InvoiceNo,
                                    CustomerName = i.CustomerName,
                                    CustomerNo = i.CustomerContact,
                                    Area = i.tblLocation.LocationNameEN,
                                    OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                    InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                    Quantity = item2.Quantity,
                                    Amount = (decimal)item2.Quantity * item2.UnitPrice,
                                    Vat = i.ServiceVat.Value,
                                    UnitPrice = item2.UnitPrice,
                                    AdditionalVat = decimal.Parse(item2.AdditionalVat.ToString("0.00")),
                                    AdditonalAmount = decimal.Parse(item2.AdditonalAmount.ToString("0.00")),
                                    ServicesName = item2.Title,
                                    Services_Quantity = item2.Quantity,
                                    Services_Title = item2.Title,
                                    Services_Total = item2.Total,
                                    Services_UnitPrice = item2.UnitPrice,
                                    Services_Vat = item2.Vat,
                                    AdditionalWorkPrice = item2.AdditionalWork,
                                    Services = services
                                };
                                if (count == 1)
                                {
                                    odr.SubTotal = item2.AdditonalAmount + (decimal)item2.Quantity * item2.UnitPrice;
                                }
                                else
                                {
                                    odr.SubTotal = item2.AdditonalAmount + (decimal)item2.Quantity * item2.UnitPrice;
                                }
                                orders.Add(odr);
                                count++;
                            }
                            foreach (vm_Invoice_Services sv in services)
                            {
                                ServiceAmount += sv.Total;
                                ServiceVat += sv.Vat;
                                AdditionalAmount += sv.AdditonalAmount;
                                AdditionalVat += sv.AdditionalVat;
                            }
                        }
                        item.Orders = orders;
                        output.Add(item);
                    }
                }
                vm_Invoice model = new vm_Invoice
                {
                    Company_Title = username,
                    Company_Email = dtSetting.CompanyEmail,
                    Company_Phone = dtSetting.CompanyPhone,
                    Company_Website = dtSetting.CompanyWebsite,
                    InvoiceDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    DueDate = toDate,
                    InvoiceNo = cls_Defaults.GenerateUniqueId(),
                    UserGroup = dtUserGroup.CompanyNameEN,
                    TotalOrders = dtOrders.RecordCount,
                    Orders = output,
                    PeriodTime = $"{fromDate} - {toDate}",
                    SubTotal = ServiceAmount + AdditionalAmount,
                    Vat = ServiceVat + AdditionalVat,
                    Additional = AdditionalAmount + AdditionalVat,
                    ServiceAmount = ServiceVat + ServiceAmount
                };
                model.Total = model.Additional + model.ServiceAmount;
                decimal contract = ContractPercent / 100m;
                decimal due_Service = ServiceAmount - ServiceAmount * contract;
                decimal due_additional = AdditionalAmount - AdditionalAmount * contract;
                decimal due_vat_Service = due_Service + due_Service * (VatPercent / 100m);
                decimal due_vat_Additonal = due_additional + due_additional * (VatPercent / 100m);
                decimal total_orderAmount = due_vat_Additonal + due_vat_Service;
                decimal pay_Sp = total_orderAmount - model.Additional;
                decimal due = (model.Due = model.ServiceAmount - pay_Sp);
                model.PayToSp = pay_Sp;
                return View(model);
            }
            catch (Exception)
            {
            }
            return View();
        }

        public async Task<ActionResult> ProviderAdmin(string fromDate, string toDate, int UserGroupId, int supplierid = 0)
        {
            try
            {
                Filters<tblOrder> filters = new Filters<tblOrder>();
                filters.Add(condition: true, (tblOrder x) => x.ReservedProvider == UserGroupId);
                filters.Add(supplierid > 0, (tblOrder x) => x.SupplierId == supplierid);
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    DateTime dateFrom = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime dateTo = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate >= dateFrom && x.InstallDate <= dateTo);
                }
                filters.Add(condition: true, (tblOrder x) => x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 9 || y.Status == 10));
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                sorts.Add(condition: true, (tblOrder x) => x.OrderId, byDescending: true);
                vm_Settings dtSetting = await objSettings.GetSetting();
                vm_GroupCompanies dtUserGroup = await objUser.GetGroupById(UserGroupId);
                Page<tblOrder> dtOrders = objSettings.GetOrders(10000, 1, sorts, filters);
                dtOrders.Results.Select((tblOrder x) => x.OrderId).ToList();
                decimal VatPercent = Convert.ToDecimal(dtSetting.Vat);
                decimal ContractPercent = Convert.ToDecimal(dtSetting.ContractPercent);
                base.ViewBag.Vat = VatPercent;
                base.ViewBag.ContractPercent = ContractPercent;
                decimal ServiceAmount = default(decimal);
                decimal ServiceVat = default(decimal);
                decimal AdditionalAmount = default(decimal);
                decimal AdditionalVat = default(decimal);
                decimal FinalAmount = default(decimal);
                decimal Vat = default(decimal);
                List<vm_Invoice_OrderDate> output = new List<vm_Invoice_OrderDate>();
                new List<int>();
                new Dictionary<string, int>();
                new Dictionary<string, decimal>();
                if (dtOrders.RecordCount > 0)
                {
                    IEnumerable<tblOrder> result = dtOrders.Results;
                    IEnumerable<DateTime> dates = result.Select((tblOrder x) => x.AddedDate).Distinct();
                    foreach (DateTime d in dates)
                    {
                        vm_Invoice_OrderDate item = new vm_Invoice_OrderDate
                        {
                            Date = d.ToString("dd/MM/yyy")
                        };
                        List<vm_Invoice_Orders> orders = new List<vm_Invoice_Orders>();
                        foreach (tblOrder i in from x in result
                                               where x.AddedDate == d
                                               orderby x.OrderId
                                               select x)
                        {
                            List<vm_Invoice_Services> services = await objSettings.ReportServices(i.OrderId, Convert.ToDecimal(dtSetting.Vat));
                            orders.Add(new vm_Invoice_Orders
                            {
                                OrderNo = i.OrderId.ToString(),
                                InvoiceNo = i.InvoiceNo,
                                CustomerName = i.CustomerName,
                                CustomerNo = i.CustomerContact,
                                Area = i.tblLocation.LocationNameEN,
                                OrderDate = i.AddedDate.ToString("dd/MM/yyy"),
                                InstallDate = i.InstallDate.Value.ToString("dd/MM/yyy"),
                                SubTotal = i.TotalAmount,
                                AdditonalAmount = i.AdditionalTotal.Value,
                                Quantity = objSettings.getorderquantity(i.OrderId),
                                Amount = i.ServiceTotal.Value,
                                Vat = i.ServiceVat.Value,
                                AdditionalVat = i.AdditionalVat.Value,
                                Services = services
                            });
                            foreach (vm_Invoice_Services sv in services)
                            {
                                ServiceAmount += sv.Total;
                                ServiceVat += sv.Vat;
                            }
                            AdditionalAmount += i.AdditionalTotal.Value;
                            AdditionalVat += i.AdditionalVat.Value;
                            Vat += ServiceVat + (i.AdditionalVat.HasValue ? i.AdditionalVat.Value : 0m);
                            FinalAmount += i.TotalAmount;
                        }
                        item.Orders = orders;
                        output.Add(item);
                    }
                }
                vm_Invoice model = new vm_Invoice
                {
                    Company_Title = dtSetting.CompanyName,
                    Company_Email = dtSetting.CompanyEmail,
                    Company_Phone = dtSetting.CompanyPhone,
                    Company_Website = dtSetting.CompanyWebsite,
                    InvoiceDate = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    DueDate = toDate,
                    InvoiceNo = cls_Defaults.GenerateUniqueId(),
                    UserGroup = dtUserGroup.CompanyNameEN,
                    TotalOrders = dtOrders.RecordCount,
                    Orders = output,
                    PeriodTime = $"{fromDate} - {toDate}",
                    SubTotal = ServiceAmount + AdditionalAmount,
                    Vat = ServiceVat + AdditionalVat,
                    Additional = AdditionalAmount + AdditionalVat,
                    ServiceAmount = ServiceVat + ServiceAmount
                };
                model.Total = model.Additional + model.ServiceAmount;
                decimal contract = ContractPercent / 100m;
                decimal due_Service = ServiceAmount - ServiceAmount * contract;
                decimal due_additional = AdditionalAmount - AdditionalAmount * contract;
                decimal due_vat_Service = due_Service + due_Service * (VatPercent / 100m);
                decimal due_vat_Additonal = due_additional + due_additional * (VatPercent / 100m);
                decimal total_orderAmount = due_vat_Additonal + due_vat_Service;
                decimal pay_Sp = total_orderAmount - model.Additional;
                decimal due = (model.Due = model.ServiceAmount - pay_Sp);
                model.PayToSp = pay_Sp;
                return View(model);
            }
            catch (Exception)
            {
            }
            return View();
        }

        public ActionResult ProviderReport(string fromDate, string toDate)
        {
            int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            string username = base.Session[cls_Defaults.Session_UserName].ToString();
            return new ActionAsPdf("Provider", new
            {
                fromDate = fromDate,
                toDate = toDate,
                UserGroupId = userGroupId,
                username = username
            });
        }

        public ActionResult SupplierReport(string fromDate, string toDate)
        {
            int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            return new ActionAsPdf("Supplier", new
            {
                fromDate = fromDate,
                toDate = toDate,
                UserGroupId = userGroupId
            });
        }
    }
}
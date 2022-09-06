using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using ClosedXML.Excel;
using EntityFrameworkPaginate;
using Microsoft.CSharp.RuntimeBinder;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Almanea.Controllers
{

    [SiteAuthorize(new string[] { "Supplier", "Admin", "SuperAdmin", "SellerStaff", "User" })]
    public class OrdersController : BaseController
    {
        private db_Settings objSettings = new db_Settings();

        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        private db_User objUser = new db_User();

        private bool isEnglish = cls_Defaults.IsEnglish;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Archieve()
        {
            return View();
        }

        public ActionResult Valid()
        {
            return View();
        }

        public ActionResult Installation()
        {
            return View();
        }

        public JsonResult GetInstallation(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string FromDate, string ToDate, int Location, int StatusId)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
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
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = objSettings.GetOrderssupplier(model.iDisplayLength, PageNo, sorts, filters, SupplierId, accountype, userid);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item in output)
                {
                    item.TotalAmount = item.ServiceTotal + item.ServiceVat;
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

        public JsonResult GetServices(int cat)
        {
            vm_Result objResult = new vm_Result();
            List<SelectListItem> result = (List<SelectListItem>)(objResult.Data = cls_DropDowns.GetService(cat));
            objResult.Count = result.Count;
            return Json(new
            {
                result = objResult.Data,
                iTotalRecords = objResult.Count,
                iTotalDisplayRecords = objResult.Count
            }, JsonRequestBehavior.AllowGet);
        }

        public static DataTable ImExport(DataTable dt, XSSFWorkbook hssfworkbook)
        {
            ISheet sheet = hssfworkbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            for (int k = 0; k < sheet.GetRow(0).LastCellNum; k++)
            {
                dt.Columns.Add(sheet.GetRow(0).Cells[k].ToString());
            }
            while (rows.MoveNext())
            {
                XSSFRow row = (XSSFRow)rows.Current;
                DataRow dr = dt.NewRow();
                for (int i = 0; i < row.LastCellNum; i++)
                {
                    ICell cell = row.GetCell(i);
                    if (cell == null)
                    {
                        dr[i] = null;
                    }
                    else
                    {
                        dr[i] = cell.ToString();
                    }
                }
                dt.Rows.Add(dr);
            }
            dt.Rows.RemoveAt(0);
            if (dt != null && dt.Rows.Count != 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    string category = dt.Rows[j]["orderid"].ToString();
                    string SellerContact = dt.Rows[j]["SellerContact"].ToString();
                    string InvoiceNo = dt.Rows[j]["InvoiceNo"].ToString();
                    string CustomerName = dt.Rows[j]["CustomerName"].ToString();
                    string LocationId = dt.Rows[j]["LocationId"].ToString();
                    string InstallDate = dt.Rows[j]["InstallDate"].ToString();
                    string CustomerContact = dt.Rows[j]["CustomerContact"].ToString();
                    string Comments = dt.Rows[j]["Comments"].ToString();
                }
            }
            return dt;
        }

        public JsonResult GetOrders(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int StatusId)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Equals(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Contains(Customer) || x.CustomerContact.Equals(Customer));
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
                if (!string.IsNullOrEmpty(InstallDate))
                {
                    DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
                }
                filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.ToString().Contains(InvoiceNo));
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == SupplierId && x.Status != 9 && x.Status != 10 && x.Status != 12 && x.Status != 11);
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = objSettings.GetOrderssupplier(model.iDisplayLength, PageNo, sorts, filters, SupplierId, accountype, userid);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item in output)
                {
                    tblProviderTimeSlot timeslot = objSettings.GetTimeslot(int.Parse(item.OrderNo));
                    int status = objSettings.GetAssignedStatus(int.Parse(item.OrderNo));
                    if (timeslot != null)
                    {
                        item.InstallDate = item.InstallDate.ToString().Substring(0, 10) + "<br/>" + cls_Defaults.get12hour(timeslot.StartHour.Value, timeslot.EndHour.Value);
                    }
                    else
                    {
                        item.InstallDate = item.InstallDate.ToString().Substring(0, 10);
                    }
                    item.TotalAmount = item.ServiceTotal + item.ServiceVat;
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

        public JsonResult GetSalesReport(vm_JqueryDataTables model, int id, string sDate, string EDate, int Companyid = 0, string branch = "", int SupplierId = 0, bool delayed = false, bool notupdated = false)
        {
            List<int> branchIdList = new List<int>();
            vm_Result objResult = new vm_Result();
            try
            {
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                SupplierId = ((SupplierId == 0) ? userGroupId : SupplierId);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                if (!string.IsNullOrEmpty(branch) && branch != "0")
                {
                    branchIdList = branch.Split(',').Select(int.Parse).ToList();
                    if (branchIdList.Count == 1)
                    {
                        int s = branchIdList.FirstOrDefault();
                        filters.Add(s > 0, (tblOrder x) => x.AddedBy == s);
                    }
                }
                filters.Add(SupplierId > 0, (tblOrder x) => x.SupplierId == SupplierId);
                if (!string.IsNullOrEmpty(sDate))
                {
                    DateTime datefrom = Convert.ToDateTime(sDate);
                    DateTime dateto = Convert.ToDateTime(EDate);
                    TimeSpan ts = new TimeSpan(23, 59, 59);
                    TimeSpan ts2 = new TimeSpan(0, 0, 0);
                    datefrom = datefrom.Add(ts2);
                    dateto = dateto.Add(ts);
                    filters.Add(condition: true, (tblOrder x) => x.AddedDate >= datefrom && x.AddedDate <= dateto);
                }
                if (userGroupTypeId == 3)
                {
                    base.ViewBag.userGroupTypeId = userGroupTypeId;
                    filters.Add(Companyid > 0, (tblOrder x) => x.ReservedProvider == Companyid);
                }
                if (delayed)
                {
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate < DateTime.Now && x.Status != 6 && x.Status != 9 && x.Status != 7);
                }
                if (notupdated)
                {
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate <= DateTime.Now && x.Status != 9 && x.Status != 7);
                }
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedBy, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.SupplierId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.InstallDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 6, (tblOrder x) => x.LocationId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 7, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = new Page<tblOrder>();
                result = ((userGroupTypeId != 2 || accountype != 17) ? objSettings.GetSalesReport(model.iDisplayLength, PageNo, sorts, filters, branchIdList) : objSettings.GetSupplierAdminSalesReport(model.iDisplayLength, PageNo, sorts, filters, userGroupTypeId, accountype, userid, branchIdList));
                List<tblOrder> lst = result.Results.ToList();
                List<vm_Order> output = Mapper.Map<List<tblOrder>, List<vm_Order>>(lst);
                foreach (vm_Order item in output)
                {
                    tblAdminUser sp = objSettings.GetSPFromSupplierAdmin(userid);
                    item.SupplierName = sp.FirstName + " " + sp.LastName;
                    item.TotalAmount = item.ServiceTotal + item.ServiceVat;
                    item.Location = (IsEnglish ? item.LocationEN : item.LocationAR);
                    item.InstallDate = item.InstallDate.Substring(0, 10);
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

        public FileResult GetSalesReportp(string id, string sDate, string EDate, int CompanyId = 0)
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
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                List<int> branchIdList = new List<int>();
                if (!string.IsNullOrEmpty(id))
                {
                    branchIdList = id.Split(',').Select(int.Parse).ToList();
                    if (branchIdList.Count == 1)
                    {
                        int s = branchIdList.FirstOrDefault();
                        filters.Add(s > 0, (tblOrder x) => x.AddedBy == s);
                    }
                }
                filters.Add(SupplierId > 0, (tblOrder x) => x.SupplierId == SupplierId);
                if (!string.IsNullOrEmpty(sDate))
                {
                    DateTime datefrom = Convert.ToDateTime(sDate);
                    DateTime dateto = Convert.ToDateTime(EDate);
                    TimeSpan ts = new TimeSpan(23, 59, 59);
                    TimeSpan ts2 = new TimeSpan(0, 0, 0);
                    datefrom = datefrom.Add(ts2);
                    dateto = dateto.Add(ts);
                    filters.Add(condition: true, (tblOrder x) => x.AddedDate >= datefrom && x.AddedDate <= dateto);
                }
                sorts.Add(condition: true, (tblOrder x) => x.AddedDate, byDescending: true);
                if (userGroupTypeId == 3)
                {
                    filters.Add(CompanyId > 0, (tblOrder x) => x.ReservedProvider == CompanyId);
                }
                Page<tblOrder> result = objSettings.GetSalesReport(10000, 1, sorts, filters, branchIdList);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_SalesOrderPrint> output = Mapper.Map<List<tblOrder>, List<vm_SalesOrderPrint>>(lst);
                foreach (vm_SalesOrderPrint item in output)
                {
                    tblAdminUser sp = objSettings.GetSPFromSupplierAdmin(userid);
                    item.ServiceProviderAssigned = sp.FirstName + " " + sp.LastName;
                    item.Location = (IsEnglish ? item.LocationEN : item.LocationAR);
                    item.Quantity = objSettings.getorderquantity(item.OrderId);
                }
                DataTable dataTable = output.ToDataTable();
                dataTable.Columns.Remove("OrderId");
                dataTable.Columns.Remove("OrderNo");
                dataTable.Columns.Remove("LocationEN");
                dataTable.Columns.Remove("LocationAR");
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

        public JsonResult GetArchieve(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location, int StatusId)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Equals(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Equals(Customer) || x.CustomerName.Contains(Customer) || x.CustomerContact.Equals(Customer));
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
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
                        filters.Add(condition: true, (tblOrder x) => x.InvoiceNo.Equals(InvoiceNo) || x.InvoiceNo.Contains(InvoiceNo) || x.OrderId == orderId);
                    }
                    else
                    {
                        filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Equals(InvoiceNo) || x.InvoiceNo.Contains(InvoiceNo));
                    }
                }
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == SupplierId && (x.Status == 9 || x.Status == 10 || x.Status == 12));
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = objSettings.GetOrderssupplier(model.iDisplayLength, PageNo, sorts, filters, SupplierId, accountype, userid);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item in output)
                {
                    int status = objSettings.GetAssignedStatus(int.Parse(item.OrderNo));
                    item.TotalAmount = item.ServiceTotal + item.ServiceVat;
                    item.CanComplain = ((accountype == 15 || accountype == 14 || accountype == 2 || accountype == 9) ? true : false);
                    item.OrderActivityDate = item.InstallDate.ToString();
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

        public JsonResult GetBranch(int groupid)
        {
            List<SelectListItem> list = (from x in objSettings.GetUserByGroupId(groupid)
                                         where x.AccountTypeId == 1 || x.AccountTypeId == 8
                                         select new SelectListItem
                                         {
                                             Value = x.UserId.ToString(),
                                             Text = x.FirstName.ToString() + " " + x.LastName.ToString()
                                         }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetValid(vm_JqueryDataTables model, string Seller, string Customer, string InvoiceNo, string InstallDate, int Location = 0, int StatusId = 0)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int SupplierId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int userGroupId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                int userid = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                int accountype = Convert.ToInt32(base.Session[cls_Defaults.Session_AccountTypeId]);
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<tblOrder> filters = new Filters<tblOrder>();
                Sorts<tblOrder> sorts = new Sorts<tblOrder>();
                filters.Add(!string.IsNullOrEmpty(Seller), (tblOrder x) => x.SellerName.Contains(Seller) || x.SellerContact.Equals(Seller));
                filters.Add(!string.IsNullOrEmpty(Customer), (tblOrder x) => x.CustomerName.Equals(Customer) || x.CustomerName.Contains(Customer) || x.CustomerContact.Equals(Customer) || x.CustomerContact.Contains(Customer));
                filters.Add(Location > 0, (tblOrder x) => x.LocationId == Location);
                filters.Add(StatusId > 0, (tblOrder x) => x.Status == StatusId);
                if (!string.IsNullOrEmpty(InstallDate))
                {
                    DateTime date = DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    filters.Add(condition: true, (tblOrder x) => x.InstallDate == date);
                }
                if (!string.IsNullOrEmpty(InvoiceNo))
                {
                    if (int.TryParse(InvoiceNo, out var orderId) && orderId > 1000)
                    {
                        filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.OrderId.Equals(orderId) || x.InvoiceNo.Contains(InvoiceNo));
                    }
                    else
                    {
                        filters.Add(!string.IsNullOrEmpty(InvoiceNo), (tblOrder x) => x.InvoiceNo.Equals(InvoiceNo) || x.InvoiceNo.Contains(InvoiceNo));
                    }
                }
                filters.Add(condition: true, (tblOrder x) => x.SupplierId == SupplierId && x.tblOrderHistories.Any((tblOrderHistory y) => y.Status == 10));
                sorts.Add(model.iSortCol_0 == 0, (tblOrder x) => x.InvoiceNo, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (tblOrder x) => x.AddedDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 3, (tblOrder x) => x.CustomerName, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (tblOrder x) => x.TotalAmount, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 5, (tblOrder x) => x.Status, (!model.sSortDir_0.Equals("asc")) ? true : false);
                Page<tblOrder> result = objSettings.GetOrderssupplier(model.iDisplayLength, PageNo, sorts, filters, SupplierId, accountype, userid);
                List<tblOrder> lst = result.Results.ToList();
                List<vm_OrderList> output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);
                foreach (vm_OrderList item in output)
                {
                    item.TotalAmount = item.ServiceTotal + item.ServiceVat;
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

        public async Task<ActionResult> Create()
        {
            _ = 1;
            try
            {
                vm_Settings setting = await objSettings.GetSetting();
                base.ViewBag.BlockDate = setting.BlockDate;
                base.ViewBag.AllowAsap = setting.PreferInstallAsap;
                base.ViewBag.Vat = setting.Vat;
                db_User objuser = new db_User();
                int UserId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserId]);
                vm_User user = await objuser.GetUserById(UserId);
                vm_Order model = new vm_Order
                {
                    SellerName = user.FirstName + " " + user.LastName,
                    SellerContact = user.MobileNo
                };
                return View(model);
            }
            catch (Exception)
            {
            }
            return RedirectToAction("Error", "Home");
        }

        public async Task<ActionResult> Edit(string Id)
        {
            try
            {
                var setting = await objSettings.GetSetting();
                ViewBag.BlockDate = setting.BlockDate;
                ViewBag.AllowAsap = setting.PreferInstallAsap;
                ViewBag.Vat = setting.Vat;

                var OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

                var model = await objSettings.GetOrderById(OrderId);
                if (model.PreferDate == 1)
                    model.InstallDate = "";
                if (model.dtInstallDate != null)
                {
                    model.InstallDate = DateTime.Parse(model.InstallDate).ToString("dd/MM/yyyy");
                }
                ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);
                model.Quantity = objSettings.getorderquantity(model.OrderId);
                ViewBag.Quantity = model.Quantity;
                ViewBag.DriverID = model.DriverId;
                ViewBag.LabourID = model.LabourId;
                //await objSettings.UpdateIsEdit(OrderId, true);

                return View(model);
            }
            catch (Exception ex) { }
            return HttpNotFound();
        }


        [HttpPost]
        public async Task<JsonResult> RemoveIsEdit(int Id)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                await objSettings.UpdateIsEdit(Id, status: false);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public async Task<ActionResult> ViewOrder(string Id)
        {
            try
            {
                var OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

                var model = await objSettings.GetOrderById(OrderId);

                ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);

                return PartialView("_ViewOrder", model);
            }
            catch (Exception ex) { }
            return HttpNotFound();
        }


        [HttpPost]
        public async Task<JsonResult> TeamCapacity1(FormCollection data)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string strServices = data["Services"].ToString();
                data["Quantity"].ToString();
                List<vm_OrderServices> services = serializer.Deserialize<List<vm_OrderServices>>(strServices);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                new db_User();
                db_Settings objSettings = new db_Settings();
                List<int> add_list = new List<int>();
                List<SelectListItem> splist = objSettings.GetProviderSetting(userGroupTypeId);
                if (splist.Count > 0)
                {
                    foreach (vm_OrderServices item in services)
                    {
                        foreach (SelectListItem sp in splist)
                        {
                            SelectListItem spassignedservice = objSettings.GetServicesmap(Convert.ToInt32(sp.Text), item.ServiceId);
                            tblTeamCapacityCalculation teamcap = objSettings.GetSPCapacity(Convert.ToInt32(sp.Text));
                            objSettings.GetEorkinhHrsy(Convert.ToInt32(sp.Text));
                            if (spassignedservice != null)
                            {
                                int spestimat = Convert.ToInt32(spassignedservice.Value);
                                int servicecalculatetime = item.Quantity * spestimat / 60;
                                if (teamcap != null)
                                {
                                    Convert.ToInt32(teamcap.CurrentCapacity);
                                }
                                if (teamcap.CurrentCapacity >= servicecalculatetime)
                                {
                                    output.Message = "true";
                                    base.ViewBag.Capaicity = 2;
                                    add_list.Add(1);
                                    break;
                                }
                                if (teamcap.DailyCapacity >= servicecalculatetime && teamcap.CurrentCapacity < servicecalculatetime)
                                {
                                    output.Message = "In Capacity case 1";
                                    base.ViewBag.Capaicity = 2;
                                    add_list.Add(2);
                                }
                                else if (teamcap.DailyCapacity <= servicecalculatetime)
                                {
                                    output.Message = "In Capacity case 2";
                                    base.ViewBag.Capaicity = 5;
                                    add_list.Add(3);
                                }
                                else
                                {
                                    output.Message = "Out Capacity";
                                    base.ViewBag.Capaicity = 1;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> TeamCapacity(FormCollection data)
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
                db_Settings objSettings = new db_Settings();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string strServices = data["Services"].ToString();
                data["Quantity"].ToString();
                List<vm_OrderServices> services = serializer.Deserialize<List<vm_OrderServices>>(strServices);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                db_User objUser = new db_User();
                int calcva = 0;
                List<int> add_list = new List<int>();
                List<SelectListItem> splist = objSettings.GetProviderSetting(userGroupTypeId);
                var records = new[]
                {
                new
                {
                    ID = "",
                    Value = ""
                }
            }.ToList();
                int spid = 0;
                foreach (SelectListItem sp in splist)
                {
                    int Index = 0;
                    foreach (vm_OrderServices item3 in services)
                    {
                        tblAdminUser activesp = objUser.getactievsp(Convert.ToInt32(sp.Text));
                        if (!activesp.Status)
                        {
                            Index = 1;
                            break;
                        }
                        tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(sp.Text));
                        int dailcapacity = 0;
                        List<tblAdminUser> labours2 = objUser.GetLaboursDopr(Convert.ToInt32(sp.Text));
                        if (labours2.Count > 0 && labours2 != null && labours2.Count > 0)
                        {
                            dailcapacity = Convert.ToInt32(labours2.Count * Convert.ToInt32(wokrhrs.KeyValue));
                        }
                        if (dailcapacity == 0)
                        {
                            Index = 1;
                            break;
                        }
                        if (wokrhrs == null || wokrhrs.KeyValue == null)
                        {
                            Index = 1;
                            break;
                        }
                        int qty = item3.Quantity;
                        tblServiceMapper spassignedservice2 = objSettings.GetServicesmap2(Convert.ToInt32(sp.Text), item3.ServiceId);
                        if (spassignedservice2 == null || spassignedservice2.Estimated == null)
                        {
                            Index = 1;
                            break;
                        }
                        int spestimat = Convert.ToInt32(spassignedservice2.Estimated);
                        if (!spassignedservice2.IsWorking)
                        {
                            output.Data2 += spassignedservice2.tblService.ServiceNameEN;
                            Index = 1;
                            break;
                        }
                        calcva += qty * spestimat / 60;
                    }
                    if (Index == 0)
                    {
                        records.Add(new
                        {
                            ID = sp.Text,
                            Value = calcva.ToString()
                        });
                    }
                    calcva = 0;
                    spid = Convert.ToInt32(sp.Text);
                }
                records = records.Skip(1).ToList();
                if (records.Count > 0)
                {
                    foreach (var itemsp in records)
                    {
                        tblSetting wokrhrs2 = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(itemsp.ID));
                        int dailcapacity2 = 0;
                        List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(spid));
                        if (labours.Count > 0 && labours != null && labours.Count > 0)
                        {
                            dailcapacity2 = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs2.KeyValue));
                        }
                        if (dailcapacity2 >= Convert.ToInt32(itemsp.Value))
                        {
                            output.Message = "true";
                            base.ViewBag.Capaicity = 2;
                            add_list.Add(1);
                            int spidd2 = Convert.ToInt32(itemsp.ID);
                            List<IGrouping<DateTime?, tblTeamCapacityCalculation>> model2 = (from x in db.tblTeamCapacityCalculations
                                                                                             where x.ServiceProviderId == (int?)spidd2 && x.InstallDate > DateTime.UtcNow
                                                                                             select x into c
                                                                                             group c by c.InstallDate).ToList();
                            if (model2.Count <= 0)
                            {
                                continue;
                            }
                            new vm_installdateblock();
                            foreach (IGrouping<DateTime?, tblTeamCapacityCalculation> iteminstaldate2 in model2)
                            {
                                tblTeamCapacityCalculation totaloccperct2 = iteminstaldate2.OrderByDescending((tblTeamCapacityCalculation x) => x.Id).FirstOrDefault();
                                iteminstaldate2.Count();
                                objSettings.Getpercensetting(Convert.ToInt32(itemsp.ID));
                                if (totaloccperct2.CurrentCapacity < Convert.ToInt32(itemsp.Value))
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
                                if (!(totaloccperct2.CurrentCapacity >= Convert.ToInt32(itemsp.Value)))
                                {
                                    continue;
                                }
                                foreach (var item2 in from c in recordsdate
                                                      group c by c.dates)
                                {
                                    foreach (var item5 in item2)
                                    {
                                        if (totaloccperct2.InstallDate.ToString() == item5.dates.ToString())
                                        {
                                            recordsdate.RemoveAll(x => x.dates == item5.dates.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else if (dailcapacity2 <= Convert.ToInt32(itemsp.Value))
                        {
                            output.Message = "In Capacity case 2";
                            objSettings.GetOrdersInstaldate(Convert.ToInt32(itemsp.ID));
                            int spidd = Convert.ToInt32(itemsp.ID);
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
                                    tblSetting getsettingper = objSettings.Getpercensetting(Convert.ToInt32(itemsp.ID));
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
                                            foreach (var item4 in item)
                                            {
                                                if (totaloccperct.InstallDate.ToString() == item4.dates.ToString())
                                                {
                                                    recordsdate.RemoveAll(x => x.dates == item4.dates.ToString());
                                                }
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
                            _ = 50 * dailcapacity2 / 100;
                            double curavailabe = (double)dailcapacity2 / (double)dailcapacity2 * 100.0;
                            double requiredperect = 50.0 - curavailabe;
                            _ = requiredperect * (double)dailcapacity2 / 100.0;
                            _ = (double)dailcapacity2 / requiredperect;
                            base.ViewBag.Capaicity = 5;
                            add_list.Add(3);
                        }
                        else
                        {
                            output.Message = "Out Capacity";
                            base.ViewBag.Capaicity = 1;
                        }
                    }
                }
                else
                {
                    output.Message = "full block";
                }
                foreach (int item6 in add_list)
                {
                    _ = item6;
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
                recordsdate = recordsdate.Skip(1).ToList();
            }
            catch (Exception)
            {
            }
            base.ViewBag.blockdatess = recordsdate;
            output.Data = recordsdate;
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> GetServicesBlockDate(FormCollection data)
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
                db_Settings objSettings = new db_Settings();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string strServices = data["Services"].ToString();
                data["Quantity"].ToString();
                List<vm_OrderServices> services = serializer.Deserialize<List<vm_OrderServices>>(strServices);
                new List<int>();
                List<int> ServiceList = services.Select((vm_OrderServices s) => s.ServiceId).ToList();
                vm_Settings setting = await objSettings.GetServicesBlockDate(ServiceList);
                base.ViewBag.ServiceBlockDate = setting.BlockDate;
            }
            catch (Exception)
            {
            }
            base.ViewBag.blockdatess = recordsdate;
            output.Data = recordsdate;
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> checkblockdate(FormCollection data)
        {
            vm_jsOutput output = new vm_jsOutput();
            int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            string strdate = data["date"].ToString();
            data["Quantity"].ToString();
            new db_User();
            db_Settings objSettings = new db_Settings();
            new AlmaneaDbEntities();
            List<SelectListItem> splist = objSettings.GetProviderSetting(userGroupTypeId);
            var records = new[]
            {
            new
            {
                ID = ""
            }
        }.ToList();
            if (splist.Count > 0)
            {
                foreach (SelectListItem sp in splist)
                {
                    int Index = 0;
                    tblSetting getsettingper = objSettings.GetBlockeddate(Convert.ToInt32(sp.Text));
                    if (getsettingper != null && getsettingper.KeyValue != null)
                    {
                        List<DateTime> bdate = getsettingper.KeyValue.Split(',').Select(Convert.ToDateTime).ToList();
                        foreach (DateTime item in bdate)
                        {
                            string ss = item.Date.ToString().Substring(0, 10);
                            if (ss == strdate)
                            {
                                Index = 1;
                                break;
                            }
                        }
                    }
                    if (Index == 0)
                    {
                        records.Add(new
                        {
                            ID = sp.Text
                        });
                    }
                    if (records.Count > 1)
                    {
                        records = records.Skip(1).ToList();
                        base.ViewBag.blockdatess = records;
                        output.Data = records;
                    }
                    else
                    {
                        output.Message = "No SP available";
                    }
                }
            }
            return Json(output);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> AddEditOrder(FormCollection data, int?[] basic)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string strServices = data["Services"].ToString();
                List<vm_OrderServices> services = serializer.Deserialize<List<vm_OrderServices>>(strServices);
                vm_Order model = data.Cast<vm_Order>();
                if (string.IsNullOrEmpty(model.InstallDate))
                {
                    if (model.PreferDate == 2)
                    {
                        output.Message = Translation.ReqInstallDate;
                    }
                    else
                    {
                        output.StatusId = 424;
                        output.Message = "error";
                    }
                    return Json(output);
                }
                if (model.OrderId > 0)
                {
                    int StatusId = (output.StatusId = await this.objSettings.EditOrder(model, services));
                    if (StatusId == -2)
                    {
                        output.Message = Translation.OrderInvoiceExist;
                    }
                }
                else
                {
                    int orderId = await this.objSettings.NewOrder(model, services);
                    if (orderId > 0)
                    {
                        string OrderNo = orderId.ToString();
                        db_Settings objSettings = new db_Settings();
                        int IsProoduction = Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
                        if (IsProoduction != 1)
                        {
                            await cls_Sms.NewOrder(orderId, OrderNo, basic, services);
                        }
                        else
                        {
                            await cls_Sms.NewOrder(orderId, OrderNo, basic, services);
                        }
                        output.StatusId = orderId;
                        output.Message = Translation.success_NewOrder;
                    }
                    else if (orderId == -2)
                    {
                        output.StatusId = orderId;
                        output.Message = Translation.OrderInvoiceExist;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public static DataTable ImExport(DataTable dt, HSSFWorkbook hssfworkbook)
        {
            ISheet sheet = hssfworkbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            for (int k = 0; k < sheet.GetRow(0).LastCellNum; k++)
            {
                dt.Columns.Add(sheet.GetRow(0).Cells[k].ToString());
            }
            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                DataRow dr = dt.NewRow();
                for (int i = 0; i < row.LastCellNum; i++)
                {
                    ICell cell = row.GetCell(i);
                    if (cell == null)
                    {
                        dr[i] = null;
                    }
                    else
                    {
                        dr[i] = cell.ToString();
                    }
                }
                dt.Rows.Add(dr);
            }
            dt.Rows.RemoveAt(0);
            if (dt != null && dt.Rows.Count != 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                }
            }
            return dt;
        }

        [HttpPost]
        public ActionResult importexcel(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return Json("please select the excel file to upload)", JsonRequestBehavior.AllowGet);
            }
            Stream streamfile = file.InputStream;
            DataTable dt = new DataTable();
            string FinName = Path.GetExtension(file.FileName);
            if (FinName != ".xls" && FinName != ".xlsx")
            {
                return Json("can only upload EXCEL documents", JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (FinName == ".xls")
                {
                    HSSFWorkbook hssfworkbook2 = new HSSFWorkbook(streamfile);
                    dt = ImExport(dt, hssfworkbook2);
                }
                else
                {
                    XSSFWorkbook hssfworkbook = new XSSFWorkbook(streamfile);
                    dt = ImExport(dt, hssfworkbook);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("import failed! " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteOrder(int Id)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.UpdateStatus(Id, 11);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        [HttpPost]
        public async Task<JsonResult> EditServiceOrder(int Id)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.EditOrderService(Id, IsActive: false);
            }
            catch (Exception)
            {
            }
            return Json(output);
        }


        public async Task<ActionResult> Details(string Id)
        {
            try
            {
                var userGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);

                ViewBag.Vat = objSettings.Vat();
                ViewBag.Id = Id;

                var OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

                var model = await objSettings.GetOrderById(OrderId);

                if (model != null)
                {
                    ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);
                    //model.InstallDate = DateTime.Parse(model.InstallDate).ToString("dd/MM/yyyy", cls_Defaults.DateTimeCulture);
                    model.InstallDate = DateTime.Parse(model.dtInstallDate.ToString()).ToString("dd/MM/yyyy", cls_Defaults.DateTimeCulture);

                    var History = await objSettings.GetHistory(OrderId);

                    //ViewBag.Additional = await objSettings.GetAdditional(OrderId);

                    if (History.Any(x => x.Status == (byte)OrderStatus.Finish))
                    {
                        var hist = History.FirstOrDefault(x => x.Status == (byte)OrderStatus.Finish && !string.IsNullOrEmpty(x.FileAttachment));
                        if (hist != null && !string.IsNullOrEmpty(hist.FileAttachment))
                            ViewBag.ExistAttachment = hist.FileAttachment;
                    }

                    if (userGroupTypeId == 2)
                        ViewBag.History = History.Where(p => p.Status != (byte)OrderStatus.Rejected).ToList();
                    else
                        ViewBag.History = History;

                    return View(model);
                }
                else
                    return RedirectToAction("Index");
            }
            catch (Exception ex) { }
            return HttpNotFound();
        }


        public ActionResult SalesDashboard(vm_OrderList obj, string submit)
        {
            try
            {
                int id = 0;
                int supplierid = 0;
                string sDate = "";
                string EDate = "";
                if (submit == null)
                {
                    sDate = DateTime.Now.ToString();
                    sDate = (obj.StartDate = sDate.Substring(0, 10));
                    EDate = (obj.EndDate = sDate);
                }
                else
                {
                    sDate = obj.StartDate.ToString();
                    EDate = obj.EndDate.ToString();
                }
                string AjentId = base.Request.Form["AjentId"];
                int TypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int TypeId2 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                if (AjentId != null && AjentId != "0" && obj.supplier != null && obj.supplier != "0")
                {
                    supplierid = int.Parse(obj.supplier);
                }
                vm_SalesMenuCount model = objSettings.GetMenuCountForSale(TypeId2, obj.AjentId, supplierid, Convert.ToDateTime(sDate), Convert.ToDateTime(EDate), obj.CompanyId);
                obj.AllOrders = model.AllOrders;
                obj.Archieve = model.Archieve;
                obj.Cancel = model.Cancel;
                obj.Reserved = model.Reserved;
                obj.Pending = model.Pending;
                obj.Completed = model.Completed;
                obj.Quantity = model.Quantity;
                if (TypeId == 3)
                {
                    base.ViewBag.UserGroupTypeId = TypeId;
                    obj.UnitOfInstallation = model.UnitOfInstallation;
                }
                return View(obj);
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        [HttpPost]
        public JsonResult SalesDashboard(vm_OrderList obj)
        {
            try
            {
                int id = 0;
                int supplierid = 0;
                string sDate = "";
                string EDate = "";
                sDate = obj.StartDate.ToString();
                EDate = obj.EndDate.ToString();
                string AjentId = obj.AjentId.ToString();
                int TypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                int TypeId2 = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                if (AjentId == null || !(AjentId != "0"))
                {
                    obj.AjentId = "";
                }
                if (obj.supplier != null && obj.supplier != "0")
                {
                    supplierid = int.Parse(obj.supplier);
                }
                vm_SalesMenuCount model = objSettings.GetMenuCountForSale(TypeId2, obj.AjentId.ToString(), supplierid, Convert.ToDateTime(sDate), Convert.ToDateTime(EDate), obj.CompanyId);
                obj.AllOrders = model.AllOrders;
                obj.Archieve = model.Archieve;
                obj.Cancel = model.Cancel;
                obj.Reserved = model.Reserved;
                obj.Pending = model.Pending;
                obj.Completed = model.Completed;
                obj.Quantity = model.Quantity;
                if (TypeId == 3)
                {
                    base.ViewBag.UserGroupTypeId = TypeId;
                    obj.UnitOfInstallation = model.UnitOfInstallation;
                    obj.ComplainbyCustomer = model.COMPLAINBYCUSTOMER;
                    obj.ComplainbySupplier = model.COMPLAINBYSUPPLIER;
                    obj.NumberOfUnitCancelled = model.NUMBEROFUNITCANCELLED;
                    obj.NumberOfUnitCompleted = model.NUMBEROFUNITCOMPLETED;
                    obj.ResponseRate = model.RESPONSERATE;
                    obj.RESPONSERATEBYAPPCONFIRMED = model.RESPONSERATEBYAPPCONFIRMED;
                }
                return Json(obj);
            }
            catch (Exception)
            {
            }
            return Json(null);
        }

        public async Task<ActionResult> SalesReport()
        {
            try
            {
                vm_OrderList obj = new vm_OrderList();
                string sDate = DateTime.Now.ToString();
                sDate = (obj.StartDate = sDate.Substring(0, 10));
                string EDate = (obj.EndDate = sDate);
                int userGroupTypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                if (userGroupTypeId == 3)
                {
                    base.ViewBag.userGroupTypeId = userGroupTypeId;
                }
                return View(obj);
            }
            catch (Exception)
            {
            }
            return HttpNotFound();
        }

        public JsonResult GetOrderCount(string id, int supplierid, string sDate, string EDate)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
                int TypeId = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupTypeId]);
                vm_SalesMenuCount model = (vm_SalesMenuCount)(output.Data = objSettings.GetMenuCountForSale(TypeId, id, supplierid, Convert.ToDateTime(sDate), Convert.ToDateTime(EDate), 0));
                output.StatusId = 1;
            }
            catch (Exception)
            {
            }
            return Json(output, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Invoice()
        {
            return View();
        }

        public async Task<JsonResult> InstallationDate(int id, string sDate)
        {
            vm_Settings setting = await objSettings.GetSetting();
            _ = base.Request.Form[""];
            int MinOrdersPerDay = Convert.ToInt32(setting.MinOrdersPerDay);
            int daycount = objSettings.getorderquantitybyDate2(sDate);
            bool canoder = false;
            int remainingcount = MinOrdersPerDay - daycount;
            if (remainingcount >= id)
            {
                canoder = true;
            }
            base.ViewBag.remainingcount = remainingcount;
            var result = new
            {
                Data = daycount,
                remain = remainingcount,
                order = canoder
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> EdiInstallationDate(int id, int previous, string sDate)
        {
            vm_Settings setting = await objSettings.GetSetting();
            _ = base.Request.Form[""];
            int MinOrdersPerDay = Convert.ToInt32(setting.MinOrdersPerDay);
            int daycount = objSettings.getorderquantitybyDate2(sDate);
            bool canoder = false;
            int presentcount = daycount - previous;
            int remainingcount = MinOrdersPerDay - presentcount;
            if (remainingcount >= id)
            {
                canoder = true;
            }
            base.ViewBag.remainingcount = remainingcount;
            var result = new
            {
                Data = daycount,
                remain = remainingcount,
                order = canoder
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSpList()
        {
            Task<List<SelectListItem>> setting = objSettings.GetSPList();
            List<SelectListItem> result = setting.Result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSPListsetting()
        {
            Task<List<SelectListItem>> setting = objSettings.GetSPListsetting();
            List<SelectListItem> result = setting.Result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetSPListsettingAssigned()
        {
            Task<List<tblProviderSettingMapper>> setting = objSettings.GetSPListsettingAssigned();
            List<tblProviderSettingMapper> result = setting.Result;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
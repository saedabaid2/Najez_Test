using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using ClosedXML.Excel;
using System.IO;

namespace Almanea.Controllers
{
    [SiteAuthorize("Supplier")]
    public class WarehouseController : BaseController
    {
        private db_Settings objSettings = new db_Settings();

        // GET: Warehouse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Orders()
        {
            return View();
        }

        public FileResult warehouseExcel(string InvoiceNo, int TypeId)
        {
            //User Group
            int UserId = Convert.ToInt32(Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);
            var AccounType = Convert.ToInt32(Session[cls_Defaults.Session_AccountTypeId]);
            int SupplierId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);

            var filters = new Filters<tblOrder>();
            var sorts = new Sorts<tblOrder>();

            filters.Add(!string.IsNullOrEmpty(InvoiceNo), x => x.InvoiceNo.Contains(InvoiceNo));
            filters.Add(!string.IsNullOrEmpty(InvoiceNo), x => x.InvoiceNo.Contains(InvoiceNo) || (x.OrderId ).ToString().Contains(InvoiceNo));

            if (TypeId > 0)
            {
                DateTime date = DateTime.Today;
                if (TypeId == 1)
                    date = DateTime.Today;
                else if (TypeId == 2)
                    date = DateTime.Today.AddDays(1);

                if (TypeId == 3)
                {
                Check_NextDate:
                    int dow = (int)date.DayOfWeek;
                    if (dow != (int)DayOfWeek.Saturday)
                    {
                        date = date.AddDays(1);

                        goto Check_NextDate;
                    }

                    filters.Add(true, x => x.InstallDate >= DateTime.Today && x.InstallDate <= date);
                }
                else if (TypeId == 4)
                {
                Check_NextDate:
                    int dow = (int)date.DayOfWeek;
                    if (dow != (int)DayOfWeek.Sunday)//Start
                    {
                        date = date.AddDays(1);

                        goto Check_NextDate;
                    }
                    var finishDate = date.AddDays(7);
                    filters.Add(true, x => x.InstallDate >= date && x.InstallDate <= finishDate);
                }
                else
                    filters.Add(true, x => x.InstallDate == date);
            }

            //Session Supplier Id
            filters.Add(true, x => x.SupplierId == SupplierId && x.Status == (int)OrderStatus.AppointmentConfirmed);
            sorts.Add(true, x => x.AddedDate, true);
            var result = objSettings.GetOrders(100000, 1, sorts, filters);

            var lst = result.Results.Where(p => p.Status != 14).ToList();
            var output = Mapper.Map<List<tblOrder>, List<vm_OrderPrint>>(lst);


            var dataTable = cls_Cast.ToDataTable<vm_OrderPrint>(output);

            dataTable.Columns.Remove("ReservedProvider");
            dataTable.Columns.Remove("OrderId");

            if (UserGroupTypeId == (int)enumGroupType.Supplier)
                dataTable.Columns.Remove("ReservedBy");

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WarehouseOrders.xlsx");
                }
            }
        }

        public JsonResult GetOrders(vm_JqueryDataTables model, string InvoiceNo, int TypeId)
        {
            var objResult = new vm_Result();
            try
            {
                int SupplierId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);

                //Page No 
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                    PageNo = (model.iDisplayStart / model.iDisplayLength) + 1;

                var filters = new Filters<tblOrder>();
                var sorts = new Sorts<tblOrder>();

                if (!string.IsNullOrEmpty(InvoiceNo))
                {
                    int n; 
                    int orderId;
                    bool isNumeric = int.TryParse(InvoiceNo, out n);
                    if (isNumeric && n > 1000)
                    {
                        orderId = (Convert.ToInt32(n));
                        filters.Add(true, x => x.InvoiceNo.Contains(InvoiceNo) || x.OrderId.Equals(orderId));
                    }
                        else
                    //    filters.Add(!string.IsNullOrEmpty(InvoiceNo), x => x.InvoiceNo.Contains(InvoiceNo));
                    filters.Add(!string.IsNullOrEmpty(InvoiceNo), x => x.InvoiceNo.Contains(InvoiceNo));
                }

                if (TypeId > 0)
                {
                    DateTime date = DateTime.Today;
                    if (TypeId == 1)
                        date = DateTime.Today;
                    else if (TypeId == 2)
                        date = DateTime.Today.AddDays(1);

                    if (TypeId == 3)
                    {
                    Check_NextDate:
                        int dow = (int)date.DayOfWeek;                       
                        if (dow != (int)DayOfWeek.Saturday)
                        {
                            date = date.AddDays(1);

                            goto Check_NextDate;
                        }
                        
                        filters.Add(true, x => x.InstallDate >= DateTime.Today && x.InstallDate <= date);
                    }
                    else if(TypeId == 4)
                    {
                    Check_NextDate:
                        int dow = (int)date.DayOfWeek;
                        if (dow != (int)DayOfWeek.Sunday)//Start
                        {
                            date = date.AddDays(1);

                            goto Check_NextDate;
                        }
                        var finishDate = date.AddDays(7);
                        filters.Add(true, x => x.InstallDate >= date && x.InstallDate <= finishDate);
                    }
                    else
                        filters.Add(true, x => x.InstallDate == date);
                }

                //Session Supplier Id
                filters.Add(true, x => x.SupplierId == SupplierId && x.Status == (int)OrderStatus.AppointmentConfirmed);

                sorts.Add(model.iSortCol_0 == 0, x => x.InvoiceNo, (model.sSortDir_0.Equals("asc") ? false : true));
                sorts.Add(model.iSortCol_0 == 1, x => x.AddedDate, (model.sSortDir_0.Equals("asc") ? false : true));
                sorts.Add(model.iSortCol_0 == 2, x => x.CustomerName, (model.sSortDir_0.Equals("asc") ? false : true));
                sorts.Add(model.iSortCol_0 == 4, x => x.TotalAmount, (model.sSortDir_0.Equals("asc") ? false : true));
                sorts.Add(model.iSortCol_0 == 5, x => x.Status, (model.sSortDir_0.Equals("asc") ? false : true));

                var result = objSettings.GetOrders(model.iDisplayLength, PageNo, sorts, filters);

                var lst = result.Results.Where(p=>p.Status!=14).ToList();
                var output = Mapper.Map<List<tblOrder>, List<vm_OrderList>>(lst);

                foreach (var item in output)
                    item.TotalAmount = item.ServiceTotal + item.ServiceVat;


                objResult.Data = output;
                objResult.Count = result.RecordCount;

            }
            catch (Exception ex)
            {
            }

            return Json(
                  new
                  {
                      aaData = objResult.Data,
                      sEcho = model.sEcho,
                      iTotalRecords = objResult.Count,
                      iTotalDisplayRecords = objResult.Count
                  }
                  , JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Details(string Id)
        {
            try
            {
                ViewBag.Vat = objSettings.Vat();
                ViewBag.Id = Id;

                var OrderId = Convert.ToInt32(EncryptDecrypt.Decrypt(Id));

                var model = await objSettings.GetOrderById(OrderId);
                if (model != null)
                {
                    ViewBag.Services = await objSettings.GetOrderServiceById(OrderId);

                    var History = await objSettings.GetHistory(OrderId);
                    
                    if (History.Any(x => x.Status == (byte)OrderStatus.Finish))
                    {
                        var hist = History.FirstOrDefault(x => x.Status == (byte)OrderStatus.Finish && !string.IsNullOrEmpty(x.FileAttachment));

                        if (hist != null && !string.IsNullOrEmpty(hist.FileAttachment))
                            ViewBag.ExistAttachment = hist.FileAttachment;
                    }

                    ViewBag.History = History;

                    return View(model);
                }
                else
                    return RedirectToAction("Orders");
            }
            catch (Exception ex) { }
            return HttpNotFound();
        }

    }
}
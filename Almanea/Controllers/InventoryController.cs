using System;
using System.Collections.Generic;
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

    public class InventoryController : BaseController
    {
        private db_Settings objSettings = new db_Settings();

        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        private db_User objUser = new db_User();

        private bool isEnglish = cls_Defaults.IsEnglish;

        public ActionResult Index()
        {
            int UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            List<SelectListItem> Mlabours = (from s in objUser.GetAavilableLabours2(0, UserGroupID)
                                             select new SelectListItem
                                             {
                                                 Text = s.FirstName + " " + s.LastName,
                                                 Value = s.UserId.ToString()
                                             }).ToList();
            base.ViewBag.LabourId2 = Mlabours;
            var Item = from x in objUser.GetItems2()
                                     where (x.UserGroupId == UserGroupID)
                                     select x;
            List<SelectListItem> Items = Item.Select((Item s) => new SelectListItem
            {
                Text = s.name + " ",
                Value = s.Id.ToString()
            }).ToList();
            base.ViewBag.Items = Items;
            return View("inventory");
        }

        public ActionResult Details(int Id)
        {
            vm_Order vm = new vm_Order();
            Inventory_Master InventoryMaster = objSettings.GetInventoryMaster(Id);
            vm.SupplierName = objSettings.GetLabourorDriveNamebyId(InventoryMaster.LabourId.Value);
            int InventoryDetails = objSettings.GetInventoryDetails(Id);
            int consumedAmount = InventoryDetails;
            vm.LocationId = InventoryMaster.Quantity.Value - consumedAmount;
            vm.OrderId = InventoryMaster.Quantity.Value;
            return View(vm);
        }

        [HttpPost]
        public async Task<JsonResult> Update(vm_InventoryMaster model, string EndDate, int notifytxt = 0)
        {
            vm_jsOutput output = new vm_jsOutput();
            try
            {
              //  int notifyme = ((notify == "on") ? 1 : 0);
                //Inventory_Master exist = objSettings.GetInventoryMasterByDate(int.Parse(model.LabourId), int.Parse(model.ItemId));
                //if (exist != null)
                //{
                //    output.Message = Translation.InventoryRepeated;
                //    return Json(output);
                //}
                int UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                vm_jsOutput vm_jsOutput = output;
                vm_jsOutput.StatusId = await objSettings.InventoryUpdate(model, UserGroupID, notifytxt);
                output.Message = "success";
            }
            catch (Exception)
            {
            }
            return Json(output);
        }

        public async Task<ActionResult> GetInventory(vm_JqueryDataTables model)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<Inventory_Master> filters = new Filters<Inventory_Master>();
                Sorts<Inventory_Master> sorts = new Sorts<Inventory_Master>();
                sorts.Add(model.iSortCol_0 == 0, (Inventory_Master x) => x.StartDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 1, (Inventory_Master x) => x.EndDate, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 2, (Inventory_Master x) => x.LabourId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 3, (Inventory_Master x) => x.ItemId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (Inventory_Master x) => x.Quantity, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (Inventory_Master x) => x.AvalQuantity, (!model.sSortDir_0.Equals("asc")) ? true : false);
                db_User objUser = new db_User();
                int UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Page<Inventory_Master> result = objUser.GetInventory(model.iDisplayLength, PageNo, filters, sorts, UserGroupID, 0, DateTime.MinValue);
                List<Inventory_Master> lst = result.Results.ToList();
                List<vm_InventoryMaster> output = Mapper.Map<List<Inventory_Master>, List<vm_InventoryMaster>>(lst);
                foreach (vm_InventoryMaster item in output)
                {
                    if (string.IsNullOrEmpty(item.AvalQuantity.ToString()))
                    {
                        item.AvalQuantity = 0;
                    }
                    item.LabourId = objSettings.GetLabourorDriveNamebyId(int.Parse(item.LabourId));
                    Item Item = objSettings.GetItemById(int.Parse(item.ItemId));
                    item.ItemId = Item.name;
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

        public async Task<ActionResult> GetInventoryDetails(vm_JqueryDataTables model, int Id)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<Inventoy_Details> filters = new Filters<Inventoy_Details>();
                Sorts<Inventoy_Details> sorts = new Sorts<Inventoy_Details>();
                db_User objUser = new db_User();
                Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Page<Inventoy_Details> result = objUser.GetInventoryDetails(model.iDisplayLength, PageNo, filters, sorts, Id);
                List<Inventoy_Details> lst = result.Results.ToList();
                List<vm_InventoryDetail> output = Mapper.Map<List<Inventoy_Details>, List<vm_InventoryDetail>>(lst);
                foreach (vm_InventoryDetail item in output)
                {
                    item.LabourId = objSettings.GetLabourorDriveNamebyId(int.Parse(item.LabourId));
                    Item Item = objSettings.GetItemById(int.Parse(item.ItemId));
                    item.ItemId = Item.name;
                    item.ServiceId = objSettings.GetServicesById2(int.Parse(item.ServiceId)).ServiceNameAR;
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

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 1)]
        public ActionResult Report(int statusId = 0, int supplierId = 0, string date = "", int location = 0)
        {
            int UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
            List<SelectListItem> Mlabours = (from s in objUser.GetAavilableLabours2(0, UserGroupID)
                                             select new SelectListItem
                                             {
                                                 Text = s.FirstName + " " + s.LastName,
                                                 Value = s.UserId.ToString()
                                             }).ToList();
            base.ViewBag.Labours = Mlabours;
            return View("InventoryReport");
        }

        public async Task<ActionResult> GetInventoryMaster(vm_JqueryDataTables model, DateTime? Date = null, int LabourId = 0)
        {
            vm_Result objResult = new vm_Result();
            try
            {
                int PageNo = 1;
                if (model.iDisplayStart >= model.iDisplayLength)
                {
                    PageNo = model.iDisplayStart / model.iDisplayLength + 1;
                }
                Filters<Inventory_Master> filters = new Filters<Inventory_Master>();
                Sorts<Inventory_Master> sorts = new Sorts<Inventory_Master>();
                sorts.Add(model.iSortCol_0 == 1, (Inventory_Master x) => x.StartDate, model.sSortDir_0.Equals("desc") && false);
                sorts.Add(model.iSortCol_0 == 2, (Inventory_Master x) => x.EndDate, model.sSortDir_0.Equals("desc") && false);
                sorts.Add(model.iSortCol_0 == 0, (Inventory_Master x) => x.LabourId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 3, (Inventory_Master x) => x.ItemId, (!model.sSortDir_0.Equals("asc")) ? true : false);
                sorts.Add(model.iSortCol_0 == 4, (Inventory_Master x) => x.Quantity, (!model.sSortDir_0.Equals("asc")) ? true : false);
                db_User objUser = new db_User();
                int UserGroupID = Convert.ToInt32(base.Session[cls_Defaults.Session_UserGroupId]);
                Page<Inventory_Master> result = objUser.GetInventory(model.iDisplayLength, PageNo, filters, sorts, UserGroupID, LabourId, Date);
                List<Inventory_Master> lst = result.Results.ToList();
                List<vm_InventoryMaster> output = Mapper.Map<List<Inventory_Master>, List<vm_InventoryMaster>>(lst);
                foreach (vm_InventoryMaster item in output)
                {
                    _ = item.Id;
                    int id = objSettings.GetInventoryDetails(item.Id);
                    if (id == -1)
                    {
                        item.Id = -1;
                    }
                    item.LabourId = objSettings.GetLabourorDriveNamebyId(int.Parse(item.LabourId));
                    Item Item = objSettings.GetItemById(int.Parse(item.ItemId));
                    item.ItemId = Item.name;
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
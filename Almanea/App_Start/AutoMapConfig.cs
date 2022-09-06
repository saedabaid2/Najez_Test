using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Almanea.Models;
using Almanea.Data;
using Almanea.BusinessLogic;
using System.Globalization;

namespace Almanea.App_Start
{
    public class AutoMapConfig
    {
        private static DateTime finishb4 = DateTime.Now.AddDays(-30);
        //private static Worker objWorker = new Worker();
        //public bool IsEnglish = (CultureInfo.CurrentCulture.Name.Equals("ar")) ? false : true;
        private static bool isEnglish = cls_Defaults.IsEnglish;

        public static void configure()
        {

            Mapper.Initialize(cfg =>
                {
                    //Groups
                    cfg.CreateMap<vm_GroupCompanies, tblUserGroupCompany>().
                    ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.Now)).
                    ForMember(dest => dest.Status, opt => opt.MapFrom(src => true)).
                    ForMember(dest => dest.IsInternal, opt => opt.MapFrom(src => false)).
                    ForMember(dest => dest.AddedBy, opt => opt.MapFrom(src => (HttpContext.Current.Session[cls_Defaults.Session_UserId] != null ? (int)HttpContext.Current.Session[cls_Defaults.Session_UserId] : 0)));

                    cfg.CreateMap<tblUserGroupCompany, vm_GroupList>().
                    ForMember(dest => dest.UserGroup, opt => opt.MapFrom(src => cls_DropDowns.GetGroupName(src.UserGroupTypeId))).
                    ForMember(dest => dest.CompanyLogo, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.CompanyLogo) ? cls_Defaults.FindImage(src.CompanyLogo, cls_Defaults.CompanyLogo) : cls_Defaults.NoImage)).
                    ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status == true ? Translation.Active : Translation.InActive)).
                     ForMember(dest => dest.IsInternalText, opt => opt.MapFrom(src => src.IsInternal == true ? Translation.True : Translation.False)).
                    //ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => objWorker.FixTrnx(src.CompanyNameEN, src.CompanyNameAR))).
                    ForMember(dest => dest.EncryptId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.UserGroupId.ToString())));

                    cfg.CreateMap<tblUserGroupCompany, vm_GroupCompanies>().
                    ForMember(dest => dest.CompanyLogo, opt => opt.MapFrom(src => cls_Defaults.FindImage(src.CompanyLogo, cls_Defaults.CompanyLogo)));

                    //Users
                    cfg.CreateMap<vm_User, tblAdminUser>().
                          ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.Now)).
                          ForMember(dest => dest.Status, opt => opt.MapFrom(src => true)).
                          //ForMember(dest => dest.AccountTypeId, opt => opt.MapFrom(src => src.AccountTypeId)). //Sub                  
                          ForMember(dest => dest.AddedBy, opt => opt.MapFrom(src => (HttpContext.Current.Session[cls_Defaults.Session_UserId] != null ? (int)HttpContext.Current.Session[cls_Defaults.Session_UserId] : 0)));

                    cfg.CreateMap<tblAdminUser, vm_User>().
                         ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => src.AddedDate.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                      ForMember(dest => dest.ProfilePic, opt => opt.MapFrom(src => cls_Defaults.FindImage(src.ProfilePic, cls_Defaults.ProfilePic))).
                        ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status == true ? Translation.Active : Translation.InActive)).
                        ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status)).
                        //ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => objWorker.GetCompanyName(src.UserGroupId))).
                        ForMember(dest => dest.UserGroup, opt => opt.MapFrom(src => cls_DropDowns.GetGroupName(src.UserGroupTypeId))).
                        ForMember(dest => dest.LaborBlockDate, opt => opt.MapFrom(src => cls_DropDowns.GetBlockDate(src.UserId))).
                        ForMember(dest => dest.EncryptId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.UserId.ToString())));

                    cfg.CreateMap<tblAdminUser, vm_UserList>().
                     ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName)).
                       ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status == true ? Translation.Active : Translation.InActive)).
                       ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status)).
                       ForMember(dest => dest.CompanyNameEN, opt => opt.MapFrom(src => src.tblUserGroupCompany.CompanyNameEN)).
                       ForMember(dest => dest.CompanyNameAR, opt => opt.MapFrom(src => src.tblUserGroupCompany.CompanyNameAR)).
                       ForMember(dest => dest.UserGroup, opt => opt.MapFrom(src => cls_DropDowns.GetGroupName(src.UserGroupTypeId))).
                       ForMember(dest => dest.EncryptId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.UserId.ToString())));


                    //Locations
                    cfg.CreateMap<vm_Locations, tblLocation>().
                          ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.Now)).
                          ForMember(dest => dest.Status, opt => opt.MapFrom(src => true)).
                          ForMember(dest => dest.UserId, opt => opt.MapFrom(src => (HttpContext.Current.Session[cls_Defaults.Session_UserId] != null ? (int)HttpContext.Current.Session[cls_Defaults.Session_UserId] : 0)));

                    cfg.CreateMap<tblLocation, vm_Locations>().
                        ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => src.AddedDate.Value.ToString("dd/MM/yyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                       ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status == true ? Translation.Active : Translation.InActive)).
                       ForMember(dest => dest.DirectionList, opt => opt.MapFrom(src => cls_DropDowns.GetDirection())).
                       ForMember(dest => dest.DirectionId, opt => opt.MapFrom(src => src.Direction)).
                       ForMember(dest => dest.AddedBy, opt => opt.MapFrom(src => src.tblAdminUser.FirstName + " " + src.tblAdminUser.LastName)).
                       //ForMember(dest => dest.Direction, opt => opt.MapFrom(src => (Direction)Enum.Parse(typeof(Direction), src.Direction))).
                       ForMember(dest => dest.EncryptId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.LocationId.ToString())));

                    //Services
                    cfg.CreateMap<vm_Services, tblService>().
                        ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.Now)).
                        ForMember(dest => dest.Status, opt => opt.MapFrom(src => true)).
                        ForMember(dest => dest.UserId, opt => opt.MapFrom(src => (HttpContext.Current.Session[cls_Defaults.Session_UserId] != null ? (int)HttpContext.Current.Session[cls_Defaults.Session_UserId] : 0)));

                    cfg.CreateMap<tblService, vm_Services>().
                       //ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => src.AddedDate.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                       ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status == true ? Translation.Active : Translation.InActive)).
                       ForMember(dest => dest.ServiceProviderName, opt => opt.MapFrom(src => cls_DropDowns.GetProviderOrSupplierByServiceId(src.ServiceId, (int)enumGroupType.Provider))).
                       ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => cls_DropDowns.GetProviderOrSupplierByServiceId(src.ServiceId, (int)enumGroupType.SellerStaff))).
                       ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => cls_DropDowns.GetCategoryList(Convert.ToInt16(src.CategoryId)))).
                       //ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.NameEn + " " + src.Category.Name)).
                       //ForMember(dest => dest.Estimatetime, opt => opt.MapFrom(src => src.Estimatetime)).
                       //ForMember(dest => dest.CategoryNameEN, opt => opt.MapFrom(src => src.Categories.NameEn )).temp
                       ForMember(dest => dest.Estimatetime, opt => opt.MapFrom(src => cls_DropDowns.GetEstimated(Convert.ToInt16(src.ServiceId)))).
                       ForMember(dest => dest.IsWorking, opt => opt.MapFrom(src => cls_DropDowns.GetIsworking(Convert.ToInt16(src.ServiceId)))).
                       //ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => cls_DropDowns.GetCategoryList(Convert.ToInt16(src.CategoryId)))).
                       ForMember(dest => dest.EncryptId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.ServiceId.ToString())));
                    //ForMember(dest => dest.AddedBy, opt => opt.MapFrom(src => src.tblAdminUser.FirstName + " " + src.tblAdminUser.LastName));temp

                    cfg.CreateMap<tblAdditionalWork, vm_AdditionalWork>().
                    ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => cls_DropDowns.GetCategoryList(Convert.ToInt16(src.CategoryrId))));
                    //Order
                    cfg.CreateMap<vm_Order, tblOrder>().
                     ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.Now)).
                     ForMember(dest => dest.SmsInArabic, opt => opt.MapFrom(src => src.SmsInArabic)).
                     ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => (!string.IsNullOrEmpty(src.InstallDate) ? DateTime.ParseExact(src.InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null))).
                      // ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)).

                      ForMember(dest => dest.Status, opt => opt.MapFrom(src => (byte)OrderStatus.NewOrder)).
                     ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => (HttpContext.Current.Session[cls_Defaults.Session_UserGroupId] != null ? (int)HttpContext.Current.Session[cls_Defaults.Session_UserGroupId] : 0))).
                     ForMember(dest => dest.AddedBy, opt => opt.MapFrom(src => (HttpContext.Current.Session[cls_Defaults.Session_UserId] != null ? (int)HttpContext.Current.Session[cls_Defaults.Session_UserId] : 0)));

                    cfg.CreateMap<tblOrder, vm_OrderList>().
                    ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => src.AddedDate.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                    ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.CustomerName + "<br/>" + src.CustomerContact)).
                    ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => (src.OrderId).ToString())).
                    ForMember(dest => dest.Invoice, opt => opt.MapFrom(src => src.InvoiceNo)).
                    ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => (isEnglish == true) ? cls_DropDowns.GetAllSupplier(src.SupplierId).CompanyNameEN : cls_DropDowns.GetAllSupplier(src.SupplierId).CompanyNameAR)).
                    ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.tblOrderServices.Where(k => k.OrderId == src.OrderId && k.tblService.IsDisplay == true).Sum(k => k.Quantity))).
                    ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => (src.PreferDate == 2 && src.InstallDate != null ? ((DateTime)src.InstallDate).ToString("dd/MM/yyy", cls_Defaults.DateTimeCulture) : Translation.ASAP) + (src.PrefferTime == 0 ? " " + src.PrefferHr + ":00 " + cls_DropDowns.GetMeridian(src.PrefferMeridian) : ""))).
                    ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => cls_DropDowns.OrderStatusName(src.Status))).
                    ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.OrderId.ToString()))).
                    ForMember(dest => dest.CanComplain, opt => opt.MapFrom(src => src.Status != (int)OrderStatus.Complete && src.tblOrderHistories.Any(y => y.Status == (int)OrderStatus.Finish && y.ActivityDate >= finishb4))).
                    ForMember(dest => dest.Direction, opt => opt.MapFrom(src => cls_DropDowns.GetDirection(src.tblLocation.Direction ?? 0).DirectionName)).
                    ForMember(dest => dest.LocationNameEN, opt => opt.MapFrom(src => src.tblLocation.LocationNameEN)).
                    ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.tblOrderServices.Where(k => k.OrderId == src.OrderId).FirstOrDefault().Unit)).
                    ForMember(dest => dest.OrderActivityDate, opt => opt.MapFrom(src => src.tblOrderHistories.Where(k => k.OrderId == src.OrderId && k.Status == 9) != null ? src.tblOrderHistories.Where(k => k.OrderId == src.OrderId && k.Status == 9).FirstOrDefault().ActivityDate.ToString() : null)).
                    ForMember(dest => dest.LocationNameAR, opt => opt.MapFrom(src => src.tblLocation.LocationNameAR));

                    cfg.CreateMap<tblOrder, vm_Order>().
                    ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => (src.PreferDate == 2 && src.InstallDate != null ? ((DateTime)src.InstallDate).ToString("dd/MM/yyy", cls_Defaults.DateTimeCulture) : Translation.ASAP) + (src.PrefferTime == 0 ? " " + src.PrefferHr + ":00 " + cls_DropDowns.GetMeridian(src.PrefferMeridian) : ""))).
                    // ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => (src.PreferDate == 2 && src.InstallDate != null ? ((DateTime)src.InstallDate).ToString("dd/MM/yyyy", cls_Defaults.DateTimeCulture) : Translation.ASAP))).
                    ForMember(dest => dest.PreferTiming, opt => opt.MapFrom(src => (src.PrefferHr != null ? src.PrefferHr + ":00 " + cls_DropDowns.GetMeridian(src.PrefferMeridian) : ""))).
                    ForMember(dest => dest.EncryptId, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.OrderId.ToString()))).
                    ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => (src.OrderId).ToString())).
                    ForMember(dest => dest.SmsInArabic, opt => opt.MapFrom(src => src.SmsInArabic)).
                    ForMember(dest => dest.dtInstallDate, opt => opt.MapFrom(src => src.InstallDate)).
                    ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => cls_DropDowns.OrderStatusName(src.Status))).
                    ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.SupplierId)).
                    ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => (isEnglish == true) ? cls_DropDowns.GetAllSupplier(src.SupplierId).CompanyNameEN : cls_DropDowns.GetAllSupplier(src.SupplierId).CompanyNameAR)).
                    ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.tblOrderServices.Where(k => k.OrderId == src.OrderId && k.tblService.IsDisplay == true).Sum(k => k.Quantity))).
                    ForMember(dest => dest.LocationEN, opt => opt.MapFrom(src => src.tblLocation.LocationNameEN)).
                    ForMember(dest => dest.LocationAR, opt => opt.MapFrom(src => src.tblLocation.LocationNameAR));

                    cfg.CreateMap<tblOrder, vm_SalesOrderPrint>().
                     ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => (src.PreferDate == 2 && src.InstallDate != null ? ((DateTime)src.InstallDate).ToString("dd/MM/yyy", cls_Defaults.DateTimeCulture) : Translation.ASAP) + (src.PrefferTime == 0 ? " " + src.PrefferHr + ":00 " + cls_DropDowns.GetMeridian(src.PrefferMeridian) : ""))).

                      ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => (src.OrderId).ToString())).

                     ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => cls_DropDowns.OrderStatusName(src.Status))).

                    ForMember(dest => dest.LocationEN, opt => opt.MapFrom(src => src.tblLocation.LocationNameEN)).
                    ForMember(dest => dest.LocationAR, opt => opt.MapFrom(src => src.tblLocation.LocationNameAR));

                    cfg.CreateMap<vm_Invoice_Orders, vm_Invoice_Orders_Print>().
                           ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice)).
                    ForMember(dest => dest.ServicesTitle, opt => opt.MapFrom(src => src.ServicesName));

                    cfg.CreateMap<vm_Invoice_Orders, vm_Invoice_Orders_Service_Print>()
                           .ForMember(dest => dest.ServicesTitle, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Services_Title) ? src.Services.FirstOrDefault().Title : src.Services_Title))
                           .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));


                    //Services
                    cfg.CreateMap<tblOrderService, vm_OrderServices>().
                            ForMember(dest => dest.ServiceNameEN, opt => opt.MapFrom(src => src.tblService.ServiceNameEN)).
                            ForMember(dest => dest.ServiceNameAR, opt => opt.MapFrom(src => src.tblService.ServiceNameAR)).
                            ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.tblService.UnitPrice));

                    //History
                    cfg.CreateMap<tblOrderHistory, vm_OrderHistoryList>().
                           ForMember(dest => dest.ActivityDate, opt => opt.MapFrom(src => src.ActivityDate.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                           ForMember(dest => dest.ActivityDt, opt => opt.MapFrom(src => src.ActivityDate.ToString("yyyyMMdd HH:mm:ss", cls_Defaults.DateTimeCulture))).
                           ForMember(dest => dest.NewStatus, opt => opt.MapFrom(src => cls_DropDowns.OrderStatusName(src.Status))).
                           ForMember(dest => dest.CommentsReschedule, opt => opt.MapFrom(src => src.CommentsReschedule)).
                           ForMember(dest => dest.PartialDelivery, opt => opt.MapFrom(src => src.Comments)).
                           //ForMember(dest => dest.Comments, opt => opt.MapFrom(src => cls_DropDowns.GetLaborDriverName(src.LabourId,src.DriverId))).
                           ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments)).
                           ForMember(dest => dest.DoneBy, opt => opt.MapFrom(src => string.Format("{0} {1} ({2})", src.tblAdminUser.FirstName, src.tblAdminUser.LastName, cls_DropDowns.GetGroupName(src.tblAdminUser.UserGroupTypeId))));

                    //Additional
                    cfg.CreateMap<tblOrderAdditionalService, vm_AdditionalService>();


                    cfg.CreateMap<tblOrder, vm_OrderPrint>().
                       ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.AddedDate.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                       ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.CustomerName + " " + src.CustomerContact)).
                       ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => (src.OrderId).ToString())).
                       ForMember(dest => dest.Invoice, opt => opt.MapFrom(src => src.InvoiceNo)).
                       ForMember(dest => dest.InstallDate, opt => opt.MapFrom(src => (src.PreferDate == 2 && src.InstallDate != null ? ((DateTime)src.InstallDate).ToString("dd/MM/yyy") : Translation.ASAP) + (src.PrefferTime == 0 ? " " + src.PrefferHr + ":00 " + cls_DropDowns.GetMeridian(src.PrefferMeridian) : ""))).
                       ForMember(dest => dest.OrderCost, opt => opt.MapFrom(src => src.TotalAmount)).
                       ForMember(dest => dest.ReservedProvider, opt => opt.MapFrom(src => src.ReservedProvider)).
                       ForMember(dest => dest.ReservedBy, opt => opt.MapFrom(src => cls_DropDowns.GetReservedBy(src.ReservedBy ?? 0))).
                       ForMember(dest => dest.Direction, opt => opt.MapFrom(src => cls_DropDowns.GetDirection(src.tblLocation.Direction ?? 0).DirectionName)).
                       ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.tblLocation.LocationNameEN)).
                       //ForMember(dest => dest.LocationNameAR, opt => opt.MapFrom(src => src.tblLocation.LocationNameAR)).
                       ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.tblOrderServices.Where(k => k.OrderId == src.OrderId).FirstOrDefault().Unit)).
                       ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.tblOrderServices.Where(k => k.OrderId == src.OrderId && k.tblService.IsDisplay == true).Sum(k => k.Quantity))).
                       ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => cls_DropDowns.OrderStatusName(src.Status)));

                    //Complain
                    cfg.CreateMap<tblOrderComplain, vm_Complain>().
                    ForMember(dest => dest.Id, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.ComplainId.ToString())));
                    ///ForMember(dest => dest.CategoryEN, opt => opt.MapFrom(src => src.tblComplainType.TitleEN)).
                    ///ForMember(dest => dest.CategoryAR, opt => opt.MapFrom(src => src.tblComplainType.TitleAR));
                    //ForMember(dest => dest.Category, opt => opt.MapFrom(src => objWorker.GetComplainType(src.ComplainTypeId)));

                    cfg.CreateMap<tblOrderComplain, vm_ComplainResponse>().
                    ForMember(dest => dest.Status, opt => opt.MapFrom(src => cls_DropDowns.ComplainStatus(src.StatusId)));
                    ///ForMember(dest => dest.CategoryEN, opt => opt.MapFrom(src => src.tblComplainType.TitleEN)).
                    ///ForMember(dest => dest.CategoryAR, opt => opt.MapFrom(src => src.tblComplainType.TitleAR));
                    //ForMember(dest => dest.Category, opt => opt.MapFrom(src => objWorker.GetComplainType(src.ComplainTypeId)));

                    cfg.CreateMap<tblOrderComplain, vm_ComplainList>().
                    ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.tblOrder.ReservedProvider)).
                    ForMember(dest => dest.Id, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.ComplainId.ToString()))).
                    ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => (src.OrderId).ToString())).
                    ForMember(dest => dest.AddedOn, opt => opt.MapFrom(src => ((DateTime)src.AddedOn).ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                    ForMember(dest => dest.ResolveOn, opt => opt.MapFrom(src => (src.StatusId == 3 || src.StatusId == 4 ? ((DateTime)src.UpdateOn).ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture) : ""))).
                    ForMember(dest => dest.Status, opt => opt.MapFrom(src => cls_DropDowns.ComplainStatus(src.StatusId))).
                    ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => (isEnglish == true) ? cls_DropDowns.GetAllSupplier(src.tblOrder.SupplierId).CompanyNameEN : cls_DropDowns.GetAllSupplier(src.tblOrder.SupplierId).CompanyNameAR)).
                    ForMember(dest => dest.ComplainBy, opt => opt.MapFrom(src => cls_DropDowns.ComplainBy(src.ComplainBy)));
                    ///ForMember(dest => dest.CategoryEN, opt => opt.MapFrom(src => src.tblComplainType.TitleEN)).

                    ///ForMember(dest => dest.CategoryAR, opt => opt.MapFrom(src => src.tblComplainType.TitleAR));
                    //ForMember(dest => dest.Category, opt => opt.MapFrom(src => objWorker.GetComplainType(src.ComplainTypeId)));

                    cfg.CreateMap<tblOrderComplain, vm_ComplainDetail>().
                    ForMember(dest => dest.Status, opt => opt.MapFrom(src => cls_DropDowns.ComplainStatus(src.StatusId))).
                    ForMember(dest => dest.InvoiceNo, opt => opt.MapFrom(src => src.tblOrder.InvoiceNo)).
                    ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments)).
                    ForMember(dest => dest.Feedback, opt => opt.MapFrom(src => src.Response)).
                    //ForMember(dest => dest.CategoryEN, opt => opt.MapFrom(src => src.tblComplainType.TitleEN)).
                    //ForMember(dest => dest.CategoryAR, opt => opt.MapFrom(src => src.tblComplainType.TitleAR)).
                    //ForMember(dest => dest.Category, opt => opt.MapFrom(src => objWorker.GetComplainType(src.ComplainTypeId))).
                    ForMember(dest => dest.SubmissioinDate, opt => opt.MapFrom(src => src.AddedOn.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture))).
                    ForMember(dest => dest.CloseDate, opt => opt.MapFrom(src => (src.UpdateOn != null ? ((DateTime)src.UpdateOn).ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture) : "")));

                    cfg.CreateMap<tblOrderComplain, vm_UserComplainList>().
                    ForMember(dest => dest.Id, opt => opt.MapFrom(src => EncryptDecrypt.Encrypt(src.ComplainId.ToString())));
                    ///ForMember(dest => dest.CategoryEN, opt => opt.MapFrom(src => src.tblComplainType.TitleEN)).
                    ///ForMember(dest => dest.CategoryAR, opt => opt.MapFrom(src => src.tblComplainType.TitleAR));
                    //ForMember(dest => dest.Category, opt => opt.MapFrom(src => objWorker.GetComplainType(src.ComplainTypeId)));

                    cfg.CreateMap<tblSM, vm_SMS>();

                    cfg.CreateMap<tblOrder, tblResetOrder>();

                    cfg.CreateMap<tblComplainType, vm_ComplainType>();
                    cfg.CreateMap<vm_ComplainType, tblComplainType>();
                });

        }
    }
}
using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class vm_Order
    {
        public int? ChannelId { get; set; }
        [Key]
        public int OrderId { get; set; }
        [Display(Name = "SellerName", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqSellerName", ErrorMessageResourceType = typeof(Translation))]
        public string SellerName { get; set; }

        [Display(Name = "SellerContact", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqSellerContact", ErrorMessageResourceType = typeof(Translation))]
        public string SellerContact { get; set; }

        [Display(Name = "AlternateMobile", ResourceType = typeof(Translation))]
        public string AlternateMobile { get; set; }

        [Display(Name = "InvoiceNo", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqInvoiceNo", ErrorMessageResourceType = typeof(Translation))]
        public string InvoiceNo { get; set; }

        [Display(Name = "CustomerName", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqCustomerName", ErrorMessageResourceType = typeof(Translation))]
        public string CustomerName { get; set; }

        [Display(Name = "CustomerContact", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqCustomerContact", ErrorMessageResourceType = typeof(Translation))]
        [RegularExpression(@"^[5]\d{8}$", ErrorMessageResourceName = "InvalidMobile", ErrorMessageResourceType = typeof(Translation))]
        public string CustomerContact { get; set; }

        [Display(Name = "CustomerLocation", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqOrdLocation", ErrorMessageResourceType = typeof(Translation))]
        public int LocationId { get; set; }

        [Display(Name = "PreferSpecific", ResourceType = typeof(Translation))]
        public byte PreferDate { get; set; }

        [Display(Name = "InstallDate", ResourceType = typeof(Translation))]
        //[RequiredIf("PreferDate", 1, ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string InstallDate { get; set; }

        [Display(Name = "PrefferTime", ResourceType = typeof(Translation))]
        public byte PrefferTime { get; set; }

        public byte Status { get; set; }

        public int UserGroupId { get; set; }
        public string Services { get; set; }
        public List<vm_OrderServices> Service { get; set; }

        public string EncryptId { get; set; }
        public int GroupTypeId { get; set; }
        public int UserId { get; set; }
        public string Location { get; set; }
        public string LocationEN { get; set; }
        public string LocationAR { get; set; }
        public string CustomerCode { get; set; }
        public int ReservedProvider { get; set; }

        [Display(Name = "OrderNo", ResourceType = typeof(Translation))]
        public string OrderNo { get; set; }
        public string CustomerSignOff { get; set; }
        public bool IsOnEdit { get; set; }

        public byte? PrefferHr { get; set; }

        public byte? PrefferMeridian { get; set; }
        public string PreferTiming { get; set; }

        [Display(Name = "SMSInArabic", ResourceType = typeof(Translation))]
        public bool SmsInArabic { get; set; }

        public decimal ServiceTotal { get; set; }
        public decimal ServiceVat { get; set; }
        public decimal AdditionalTotal { get; set; }
        public decimal AdditionalVat { get; set; }

        public decimal TotalAmount { get; set; }
        public string AddedDate { get; set; }

        public decimal Vat { get; set; }
        public DateTime? dtInstallDate { get; set; }
        [Display(Name = "Comments", ResourceType = typeof(Translation))]
        public string Comments { get; set; }
        public string StatusText { get; set; }
        public int Quantity { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int Unit { get; set; }
        [Required]
        public List<int> LabourIds { get; set; }
        public int LabourId { get; set; }
        public int DriverId { get; set; }
        public string InvoiceImages { get; set; }


    }
    public class vm_installdateblock
    {
        public string date { get; set; }
    }

    public class vm_OrderList
    {
        public tblProviderTimeSlot tblProviderTimeSlot { get; set; }
        public int CompanyId { get; set; }
        public string OrderActivityDate { get; set; }
        public string OrderNo { get; set; }
        public string OrderId { get; set; }
        public string Invoice { get; set; }
        public string AddedDate { get; set; }
        public string InstallDate { get; set; }
        public string StartHour { get; set; }
        public string EndHour { get; set; }

        public string Customer { get; set; }
        public string Direction { get; set; }

        public string Location { get; set; }
        public string LocationNameEN { get; set; }
        public string LocationNameAR { get; set; }

        public int Status { get; set; }
        public string StatusText { get; set; }

        public int? ReservedProvider { get; set; }
        public string ReservedBy { get; set; }
        public decimal ServiceTotal { get; set; }
        public decimal ServiceVat { get; set; }

        public decimal TotalAmount { get; set; }
        public bool CanComplain { get; set; }
        [Display(Name = "Branch", ResourceType = typeof(Translation))]
        public string Branch { get; set; }
        public string AjentId { get; set; }
        public string supplier { get; set; }
        [Display(Name = "StartDate", ResourceType = typeof(Translation))]
        public string StartDate { get; set; }
        [Display(Name = "EndDate", ResourceType = typeof(Translation))]
        public string EndDate { get; set; }
        [Display(Name = "CustomerLocation", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqOrdLocation", ErrorMessageResourceType = typeof(Translation))]

        public int LocationId { get; set; }
        public int Unit { get; set; }
        public int AllOrders { get; set; }
        public int Completed { get; set; }
        public int Archieve { get; set; }
        public int Cancel { get; set; }
        public int Quantity { get; set; }
        public int Pending { get; set; }
        public int Reserved { get; set; }
        public int UnitOfInstallation { get; set; }
        public int ComplainbyCustomer { get; set; }
        public int ComplainbySupplier { get; set; }
        public int NumberOfUnitCancelled { get; set; }
        public int NumberOfUnitCompleted { get; set; }
        public int ResponseRate { get; set; }
        public int RESPONSERATEBYAPPCONFIRMED { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceImages { get; set; }
        public string DeliveryStatus { get; set; }
        public string CustomerLngLat { get; set; }
    }

    public class vm_OrderPrint
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public string Invoice { get; set; }
        public string OrderDate { get; set; }
        public string InstallDate { get; set; }

        public string Customer { get; set; }

        public decimal Quantity { get; set; }
        public string OrderCost { get; set; }

        public string StatusText { get; set; }
        public int ReservedProvider { get; set; }
        public string ReservedBy { get; set; }
        public string LocationName { get; set; }
        public string Direction { get; set; }
        public int Unit { get; set; }
    }

    public class vm_SalesOrderPrint
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public string ServiceProviderAssigned { get; set; }
        public string SellerName { get; set; }
        public string AddedDate { get; set; }
        public string LocationEN { get; set; }
        public string LocationAR { get; set; }

        public string InstallDate { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
        public string StatusText { get; set; }
    }
    public class vm_OrderCount
    {

        public int TotalOrders { get; set; }
        public int TotalQuantity { get; set; }
        public int TotalReserved { get; set; }
        public int TotalPending { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancelled { get; set; }
    }
    public class vm_ServiceItem
    {
        public int ServiceId { get; set; }
        public int ItemId { get; set; }
        public int quantity { get; set; }
    }
    public class vm_Item
    {
        public int id { get; set; }

        public string name { get; set; }

        public string name_en { get; set; }
    }
    public class vm_Capacity
    {

        public int Id { get; set; }
        public Nullable<int> ServiceProviderId { get; set; }
        public Nullable<int> DailyCapacity { get; set; }
        public Nullable<int> ConsumedCapacity { get; set; }
        public Nullable<System.DateTime> Updatedate { get; set; }
        public Nullable<int> CurrentCapacity { get; set; }
    }
    public class vm_OrderServices
    {
        [Key]
        public int OrderServiceId { get; set; }

        public int OrderId { get; set; }


        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }

        public string ServiceName { get; set; }

        public string ServiceNameEN { get; set; }
        public string ServiceNameAR { get; set; }

        public decimal Price { get; set; }

        public int? IsAdditional { get; set; }
        [DefaultValue(0)]
        public int Unit { get; set; }
        public int LabourId { get; set; }
        public int? DriverId { get; set; }
    }

    public class vm_OrderAdditionalWork
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int AdditionalWorkId { get; set; }

        public string AdditionalWorkName { get; set; }

        public string AdditionalWorkNameEN { get; set; }
        public string AdditionalWorkNameAR { get; set; }

        public decimal Price { get; set; }
        public int LabourId { get; set; }
    }

    public class vm_OrderStatus
    {
        public int OrderId { get; set; }

        [Display(Name = "OrderNewStatus", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqStatus", ErrorMessageResourceType = typeof(Translation))]
        public byte Status { get; set; }

        [Display(Name = "AssignDriver", ResourceType = typeof(Translation))]
        [Required, Range(1, int.MaxValue, ErrorMessage = "Driver is required")]
        public int DriverId { get; set; }


        [Display(Name = "AssignLabour", ResourceType = typeof(Translation))]
        [Required, Range(1, int.MaxValue, ErrorMessage = "Labour is required")]
        public string LabourId { get; set; }

        [Display(Name = "Comments", ResourceType = typeof(Translation))]
        public string Comments { get; set; }
        public string FileName { get; set; }
        public string TimeSlot { get; set; }
        public string InstallDate { get; set; }

        public string CustomerSignOff { get; set; }
        public string InvoiceImage { get; set; }
    }


    public class vm_OrderSchedule
    {
        public int OrderId { get; set; }

        [Display(Name = "InstallDate", ResourceType = typeof(Translation))]
        public string InstallDate { get; set; }

        [Required]
        [Range(1, 12, ErrorMessageResourceName = "InvalidHour", ErrorMessageResourceType = typeof(Translation))]
        public byte PrefferHr { get; set; }


        public string TimeSlot { get; set; }

        [Required]
        public byte PrefferMeridian { get; set; }

        public byte PreferDate { get; set; }

        public string Comments { get; set; }
        public int Status { get; set; }
        public List<int> LabourId { get; set; }
        public List<int> LabourId2 { get; set; }
        public List<string> OrdersAssigned { get; set; }
        public int? DriverId { get; set; }
        public List<int> DriverIds { get; set; }
        public int? TotalServiceTime { get; set; }
    }

    public class vm_AdditionalService
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string ServiceName { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }

        public decimal Price { get; set; }
        public decimal Vat { get; set; }

        public decimal SpareParts { get; set; }
        public decimal AdditionalWorkPrice { get; set; }

    }

    public class vm_OrderHistoryList
    {
        public string ActivityDate { get; set; }
        public string NewStatus { get; set; }
        public byte Status { get; set; }
        public string Comments { get; set; }
        public string DoneBy { get; set; }
        public string ActivityDt { get; set; }
        public string FileAttachment { get; set; }
        public string OrderDate { get; set; }
        public string CommentsReschedule { get; set; }
        public string PartialDelivery { get; set; }
    }

    public class vm_OrderConfirmCode
    {
        public int OrderId { get; set; }


        [Required(ErrorMessageResourceName = "ReqCode", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "Code", ResourceType = typeof(Translation))]
        public string Code { get; set; }
        public int LabourId { get; set; }
    }

    public class VmOrderCalendar
    {
        public string title { get; set; }
        public string url { get; set; }

        public string start { get; set; }
        public string color { get; set; }
        public string textColor { get; set; }

        public string className { get; set; }
        // public DateTime end { get; set; }
    }

}
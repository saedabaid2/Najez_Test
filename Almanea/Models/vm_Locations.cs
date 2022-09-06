using Almanea.BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Almanea.Models
{
    public class vm_Locations
    {
        [Key]
        public int LocationId { get; set; }
        [Display(Name = "LocationEN", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqLocation", ErrorMessageResourceType = typeof(Translation))]
        public string LocationNameEN { get; set; }
        [Display(Name = "LocationAR", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqLocation", ErrorMessageResourceType = typeof(Translation))]
        public string LocationNameAR { get; set; }
        public bool Status { get; set; }
        public string AddedDate { get; set; }

        public string AddedBy { get; set; }

        public string StatusText { get; set; }
        public string EncryptId { get; set; }
        public IList<vm_Direction> DirectionList { get; set; }
        public int DirectionId { get; set; }
        public string UserGroupId { get; set; }

    }

    public class vm_Direction
    {
        public long Id { get; set; }
        public string DirectionName { get; set; }
    }

    public class vm_Services
    {
        [Key]
        public string EncryptId { get; set; }
        public int ServiceId { get; set; }
        [Display(Name = "ServiceProvider", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "SelectServiceProvider", ErrorMessageResourceType = typeof(Translation))]
        public string ServiceProviderId { get; set; }
        public string ServiceProviderName { get; set; }

        [Display(Name = "CategoryName", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "SelectCategory", ErrorMessageResourceType = typeof(Translation))]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryNameEN { get; set; }

        [Display(Name = "Supplier", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }

        [Display(Name = "ServiceEN", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqService", ErrorMessageResourceType = typeof(Translation))]
        public string ServiceNameEN { get; set; }

        [Display(Name = "ServiceAR", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqService", ErrorMessageResourceType = typeof(Translation))]
        public string ServiceNameAR { get; set; }

        public string ServiceName { get; set; }


        [Display(Name = "UnitPriceExclusiveVat", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqUnitPrice", ErrorMessageResourceType = typeof(Translation))]
        public decimal UnitPrice { get; set; }

        public bool Status { get; set; }
        public bool IsDisplay { get; set; }
        public string AddedDate { get; set; }

        public string AddedBy { get; set; }
        public string StatusText { get; set; }
        public bool IsWorking { get; set; }
        public string Estimatetime { get; set; }
        public int ServId { get; set; }
    }
    public class vm_Items
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Orders
    {
        public string OrderId { get; set; }
    }
    public class vm_ServicesMapper
    {
        [Key]
        public string EncryptId { get; set; }

        public int ServiceId { get; set; }

        [Display(Name = "ServiceProvider", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public int? ServiceProviderId { get; set; }

        public int? SupplierId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string Estimated { get; set; }

        public bool isworking { get; set; }

        public string ServiceNameEN { get; set; }

        public virtual tblService tblService { get; set; }

        public bool InventoryRequired { get; set; }
    }
    public class vm_AdditionalWork
    {
        [Key]
        public int AdditionalWorkId { get; set; }



        [Display(Name = "Additional Work Name English ")]
        [Required(ErrorMessageResourceName = "Enterthenameofadditionalwork", ErrorMessageResourceType = typeof(Translation))]
        public string AdditionalWorkNameEN { get; set; }

        [Display(Name = "Additional Work Name Arabic")]
        [Required(ErrorMessageResourceName = "Enterthenameofadditionalwork", ErrorMessageResourceType = typeof(Translation))]
        public string AdditionalWorkNameAR { get; set; }

        [Display(Name = "Price")]
        [Required(ErrorMessageResourceName = "Enterthepriceofadditionalwork", ErrorMessageResourceType = typeof(Translation))]
        public decimal Price { get; set; }

        public int UserGroupId { get; set; }
        public short? CategoryId { get; set; }
        public string CategoryName { get; set; }

    }

    public class vm_FileAttach
    {
        public int OrderId { get; set; }
        [Display(Name = "Comments", ResourceType = typeof(Translation))]
        public string Comments { get; set; }
        [Display(Name = "UploadAttachment", ResourceType = typeof(Translation))]
        public string CustomerSignOff { get; set; }
        [Display(Name = "UploadAttachment", ResourceType = typeof(Translation))]
        public string InvoiceImage { get; set; }
        [Display(Name = "AssignLabour", ResourceType = typeof(Translation))]
        [Required, Range(1, int.MaxValue, ErrorMessage = "Labour is required")]
        public string LabourId { get; set; }
    }

    public class vm_ProviderSettings
    {
        public long Id { get; set; }
        public Nullable<int> ServiceProviderId { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public bool IsInternal { get; set; }
    }

    public class vm_Settings
    {
        [Display(Name = "Vat5", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqVat", ErrorMessageResourceType = typeof(Translation))]
        public string Vat { get; set; }

        [Display(Name = "OrderExpNotifyDuration", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string OrderDuration { get; set; }

        [Display(Name = "SyanahCompany", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string CompanyName { get; set; }

        [Display(Name = "SyanahPhone", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string CompanyPhone { get; set; }

        [Display(Name = "SyanahEmail", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string CompanyEmail { get; set; }

        [Display(Name = "SyanahWebsite", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string CompanyWebsite { get; set; }

        [Display(Name = "ContractPercent", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string ContractPercent { get; set; }

        [Display(Name = "OrderShowDuration", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public int OrderShow { get; set; }

        [Display(Name = "ShowPreferInstall", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public bool PreferInstallAsap { get; set; }

        [Display(Name = "MinOrdersPerDay", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public int MinOrdersPerDay { get; set; }

        public int MaxOrdersPerDay { get; set; }
        [Display(Name = "IsProoduction", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public int IsProoduction { get; set; }

        [Display(Name = "BlockDate", ResourceType = typeof(Translation))]
        //[Required(ErrorMessageResourceName = "SelectBlockDate", ErrorMessageResourceType = typeof(Translation))]
        public string BlockDate { get; set; }

        [Display(Name = "SessionTimeOut", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public int SessionTimeOut { get; set; }

        [Display(Name = "MaxNumberOfUnit", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public int MaxNumberOfUnit { get; set; }
        public bool Autodispatch { get; set; }

    }

    public class vm_Email
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ContractPercent", ResourceType = typeof(Translation))]
        public string KeyName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "SubjectEN", ResourceType = typeof(Translation))]
        public string SubjectEN { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "SubjectAR", ResourceType = typeof(Translation))]
        public string SubjectAR { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        [AllowHtml]
        [Display(Name = "EmailHtmlEN", ResourceType = typeof(Translation))]
        public string EmailTextEN { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        [AllowHtml]
        [Display(Name = "EmailHtmlAR", ResourceType = typeof(Translation))]
        public string EmailTextAR { get; set; }

    }
}
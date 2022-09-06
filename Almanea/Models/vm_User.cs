using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Foolproof;

namespace Almanea.Models
{

    public class vm_Login
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Enter Email")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Enter Password")]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class vm_Forgot
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Enter Email")]
        public string UserName { get; set; }
    }

    public class vm_User
    {
        [Key]
        public int UserId { get; set; }
        [Display(Name = "FirstName", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqFirstName", ErrorMessageResourceType = typeof(Translation))]
        public string FirstName { get; set; }
        [Display(Name = "LastName", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqLastName", ErrorMessageResourceType = typeof(Translation))]
        public string LastName { get; set; }
        [System.Web.Mvc.Remote("UserEmailExist", "User", AdditionalFields = "UserId", ErrorMessageResourceName = "EmailExist", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "Email", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqEmail", ErrorMessageResourceType = typeof(Translation))]
        [EmailAddress(ErrorMessageResourceName = "ReqEmail", ErrorMessageResourceType = typeof(Translation))]
        public string Email { get; set; }
        [System.Web.Mvc.Remote("UserMobileNoExist", "User", AdditionalFields = "UserId", ErrorMessageResourceName = "ExitMobileNo", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "Telephone", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqTelePhone", ErrorMessageResourceType = typeof(Translation))]
        [MaxLength(50)]
        [RegularExpression(@"^[5]\d{8}$", ErrorMessageResourceName = "InvalidMobile", ErrorMessageResourceType = typeof(Translation))]
        public string MobileNo { get; set; }
        public bool StatusId { get; set; }
        [RegularExpression(@"^[0-9]{10}$", ErrorMessageResourceName = "InvalidId", ErrorMessageResourceType = typeof(Translation))]
        [MaxLength(10, ErrorMessageResourceName = "IqaamaLength", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "IqaamaNo", ResourceType = typeof(Translation))]
        public string IqaamaNo { get; set; }

        [Display(Name = "UserGroupType", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqUserType", ErrorMessageResourceType = typeof(Translation))]
        public byte UserGroupTypeId { get; set; }

        [Display(Name = "UserGroup", ResourceType = typeof(Translation))]
        [RequiredIf("IsNotAdmin", true, ErrorMessageResourceName = "ReqUserGroup", ErrorMessageResourceType = typeof(Translation))]
        public int? UserGroupId { get; set; }

        [StringLength(40, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "Password", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqPassword", ErrorMessageResourceType = typeof(Translation))]
        public string Password { get; set; }

        [Display(Name = "ConfirmPassword", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqPassword", ErrorMessageResourceType = typeof(Translation))]
        [Compare("Password", ErrorMessageResourceName = "PasswordUnMatch", ErrorMessageResourceType = typeof(Translation))]
        public string ConfirmPassword { get; set; }

        [Display(Name = "AccountType", ResourceType = typeof(Translation))]
        [RequiredIf("IsNotAdmin", true, ErrorMessageResourceName = "ReqAccountType", ErrorMessageResourceType = typeof(Translation))]
        public int AccountTypeId { get; set; }

        [Display(Name = "ProfilePic", ResourceType = typeof(Translation))]
        public string ProfilePic { get; set; }

        public bool IsNotAdmin
        {
            get
            {
                return UserGroupTypeId == 3 || UserGroupTypeId == 4;
            }
        }

        public bool LabourIsDriver { get; set; }
        public string StatusText { get; set; }

        public string EncryptId { get; set; }
        public string AddedDate { get; set; }
        public string UserGroup { get; set; }

        public string CompanyName { get; set; }
        public string LaborBlockDate { get; set; }
        public List<vm_LaborBlockedDate> LaborBlokeddate { get; set; }
    }

    public class vm_LaborBlockedDate
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public Nullable<int> LabourId { get; set; }
        public string InactiveDates { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }

        public virtual tblAdminUser tblAdminUser { get; set; }
        public virtual tblUserGroupCompany tblUserGroupCompany { get; set; }

    }


    public class vm_UserList
    {
        [Key]
        public int UserId { get; set; }

        public int AccountTypeId { get; set; }

        public string AccountName { get; set; }

        public string EncryptId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string MobileNo { get; set; }
        public bool StatusId { get; set; }


        public string StatusText { get; set; }

        public string UserGroup { get; set; }
        public int UserGroupId { get; set; }

        public string CompanyName { get; set; }
        public string CompanyNameEN { get; set; }
        public string CompanyNameAR { get; set; }
        public string LastLoggedIn { get; set; }
        public bool IsLogin { get; set; }

    }
    public class vm_EditPassword
    {
        [Key]
        public string UserId { get; set; }
        [DataType(DataType.Password)]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "CurrentPassword")]
        [Required(ErrorMessage = "Enter CurrentPassword")]
        public string CurrentPassword { get; set; }


        [DataType(DataType.Password)]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Enter Password")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Enter Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password should be same")]
        public string ConfirmPassword { get; set; }
    }

    public class vm_GroupCompanies
    {

        [Key]
        public int UserGroupId { get; set; }

        [Display(Name = "CompanyNameEN", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqCompanyNameEN", ErrorMessageResourceType = typeof(Translation))]
        public string CompanyNameEN { get; set; }

        [Display(Name = "CompanyNameAR", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqCompanyNameAR", ErrorMessageResourceType = typeof(Translation))]
        public string CompanyNameAR { get; set; }

        [Display(Name = "UserGroupType", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqUserGroupType", ErrorMessageResourceType = typeof(Translation))]
        public byte UserGroupTypeId { get; set; }

        [RegularExpression(@"^[5]\d{8}$", ErrorMessageResourceName = "InvalidMobile", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "Telephone", ResourceType = typeof(Translation))]
        [MaxLength(50)]
        public string Telephone { get; set; }

        [Display(Name = "Fax", ResourceType = typeof(Translation))]
        [MaxLength(50)]
        public string Fax { get; set; }

        [System.Web.Mvc.Remote("GroupEmailExist", "User", AdditionalFields = "UserGroupId", ErrorMessageResourceName = "EmailExist", ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = "Email", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "ReqEmail", ErrorMessageResourceType = typeof(Translation))]
        [EmailAddress(ErrorMessageResourceName = "ReqEmail", ErrorMessageResourceType = typeof(Translation))]
        public string Email { get; set; }

        public string CompanyLogo { get; set; }

        public bool Status { get; set; }
        public bool IsInternal { get; set; }

        [Display(Name = "ContractPercent", ResourceType = typeof(Translation))]
       // [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public decimal Contract { get; set; }
    }

    public class vm_GroupList
    {

        [Key]
        public int UserGroupId { get; set; }

        public string CompanyName { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public string CompanyLogo { get; set; }

        public string StatusText { get; set; }

        public string UserGroup { get; set; }

        public bool Status { get; set; }
        public bool IsInternal { get; set; }

        public string IsInternalText { get; set; }
        public string EncryptId { get; set; }
        public string CompanyNameEN { get; set; }
        public string CompanyNameAR { get; set; }
    }


    public class vm_Complain
    {
        [Key]
        [Display(Name = "Complain", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "SelectComplain", ErrorMessageResourceType = typeof(Translation))]
        public int ComplainId { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Translation))]
        //[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string Subject { get; set; }

        [Display(Name = "ComplainCategory", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "SelectComplainCategory", ErrorMessageResourceType = typeof(Translation))]
        public int CategoryId { get; set; }

        [Display(Name = "Complain", ResourceType = typeof(Translation))]
        //[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
        public string Complain { get; set; }

        public int OrderId { get; set; }

        public byte ComplainBy { get; set; }
        public string Category { get; set; }
        public string Id { get; set; }

        public string CategoryEN { get; set; }
        public string CategoryAR { get; set; }
    }

    public class vm_ComplainDetail
    {
        [Key]
        public int Id { get; set; }

        public string Subject { get; set; }

        public string Status { get; set; }

        public string InvoiceNo { get; set; }

        public string ComplainId { get; set; }

        public string SubmissioinDate { get; set; }

        public string CloseDate { get; set; }

        public string Comments { get; set; }
        public string Category { get; set; }
        public string Feedback { get; set; }

        public string CategoryEN { get; set; }

        public string CategoryAR { get; set; }
    }

    public class vm_ComplainResponse
    {
        public int Id { get; set; }
        [Display(Name = "ComplainId", ResourceType = typeof(Translation))]
        public string ComplainId { get; set; }
        [Display(Name = "OrderNo", ResourceType = typeof(Translation))]
        public int OrderId { get; set; }

        public byte StatusId { get; set; }
        [Display(Name = "Status", ResourceType = typeof(Translation))]
        public string Status { get; set; }

        public byte ComplainBy { get; set; }

        [Display(Name = "Complain", ResourceType = typeof(Translation))]
        public string Comments { get; set; }

        [Display(Name = "ComplainCategory", ResourceType = typeof(Translation))]
        public string Category { get; set; }

        [Display(Name = "CommentTo", ResourceType = typeof(Translation))]
        public string Response { get; set; }

        [Display(Name = "Response", ResourceType = typeof(Translation))]
        public string Response2 { get; set; }
        
        public string CategoryEN { get; set; }
        public string CategoryAR { get; set; }
    }

    public class vm_ComplainList
    {
        public string Id { get; set; }
        public int ComplainId { get; set; }
        public string Comments { get; set; }

        public int ProviderId { get; set; }
        public string Provider { get; set; }

        public string Status { get; set; }

        public string Response { get; set; }
        public string OrderNo { get; set; }

        public string AddedOn { get; set; }
        public string ResolveOn { get; set; }

        public string Category { get; set; }
        public string CategoryEN { get; set; }
        public string CategoryAR { get; set; }
        public string ComplainBy { get; set; }
        public int SupplierId { get; set; } 
        public string SupplierName { get; set; } 
    }

    public class vm_UserComplainList
    {
        public string Id { get; set; }
        public int ComplainId { get; set; }

        public string Category { get; set; }
        public string CategoryEN { get; set; }
        public string CategoryAR { get; set; }
    }

    public class vm_SMS
    {
        public int Id { get; set; }
        public string KeyName { get; set; }
        public string SMSTextEN { get; set; }
        public string SMSTextAR { get; set; }
    }
    public class vm_PushNotification
    {
        public int Id { get; set; }
        public string KeyName { get; set; }
        public string PushNotificationTextEN { get; set; }
        public string PushNotificationTextAR { get; set; }
    }

    public class vm_Complains
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string InvoiceNo { get; set; }
        public int ComplainId { get; set; }

        public string AddedOn { get; set; }
        public string CloseDate { get; set; }

        public string Comments { get; set; }
    }

    public class vm_ComplainType
    {
        [Key]
        public int ComplainTypeId { get; set; }

        [Required(ErrorMessage = "Required")]
        public byte UserGroupTypeId { get; set; }

        [Display(Name = "CategoryEN", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "EnterCategoryInEnglish", ErrorMessageResourceType = typeof(Translation))]
        public string TitleEN { get; set; }

        [Display(Name = "CategoryAR", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "EnterCategoryInArabic", ErrorMessageResourceType = typeof(Translation))]
        public string TitleAR { get; set; }

        [Display(Name = "SelectCategory", ResourceType = typeof(Translation))]
        [Required(ErrorMessageResourceName = "SelectCategory", ErrorMessageResourceType = typeof(Translation))]
        public int ComplainCategoryId { get; set; }

        public bool? IsActive { get; set; }
    }

    public class vm_ComplainHistory
    {
        public string AddedOn { get; set; }
        public string Comments { get; set; }
        public string SendBy { get; set; }
    }
}
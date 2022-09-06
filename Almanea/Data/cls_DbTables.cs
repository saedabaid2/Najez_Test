using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Almanea.Data
{

    public class tblOrder
    {
        [Key]
        public int OrderId { get; set; }
        [MaxLength(500)]
        public string SellerName { get; set; }
        [MaxLength(50)]
        public string SellerContact { get; set; }
        [MaxLength(150)]
        public string InvoiceNo { get; set; }
        [MaxLength(500)]
        public string CustomerName { get; set; }
        [MaxLength(50)]
        public string CustomerContact { get; set; }
        public int LocationId { get; set; }

        public byte PreferDate { get; set; }
        public DateTime? InstallDate { get; set; }

        public byte PrefferTime { get; set; }
        public int? PrefferHr { get; set; }  //Hour
        public byte? PrefferMeridian { get; set; }  //1 AM  2 PM

        public byte Status { get; set; }

        public DateTime AddedDate { get; set; }

        public int SupplierId { get; set; }

        public int ReservedProvider { get; set; }
        public int AddedBy { get; set; }

        public int CustomerCode { get; set; }

        public decimal Vat { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsOnEdit { get; set; }

        public string CustomerSignOff { get; set; }

        public virtual tblLocations OrderLocations { get; set; }

        public tblOrder()
        {
            this.OrderServices = new HashSet<tblOrderServices>();
            this.OrderHistory = new HashSet<tblOrderHistory>();
            this.OrderAddional = new HashSet<tblAdditionalServices>();
        }

        public virtual ICollection<tblOrderServices> OrderServices { get; set; }
        public virtual ICollection<tblOrderHistory> OrderHistory { get; set; }

        public virtual ICollection<tblAdditionalServices> OrderAddional { get; set; }
    }

    public class tblOrderServices
    {
        [Key]
        public int OrderServiceId { get; set; }

        public int OrderId { get; set; }

        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }

        public int? IsAdditional { get; set; }

        public virtual tblOrder OrderServiceOrder { get; set; }
        public virtual tblServices OrderServices { get; set; }
    }

    public class tblOrderHistory
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public DateTime ActivityDate { get; set; }
        public byte Status { get; set; }

        public int UserId { get; set; }
        [MaxLength(2500)]
        public string Comments { get; set; }

        public string FileAttachment { get; set; }

        public virtual tblOrder OrderHistory { get; set; }
        public virtual tblAdminUser AdminHistory { get; set; }
    }

    public class tblOrderRelease
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public DateTime ActivityDate { get; set; }
        public byte Status { get; set; }

        public int UserId { get; set; }
        [MaxLength(2500)]
        public string Comments { get; set; }

        public string FileAttachment { get; set; }
    }

    public class tblAdditionalServices
    {
        [Key]
        public int AdditionalServiceId { get; set; }

        public int OrderId { get; set; }

        public int UserId { get; set; }

        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string SpareParts { get; set; }
        public DateTime AddedOn { get; set; }
        public bool Status { get; set; }

        public virtual tblOrder OrderAdditional { get; set; }
    }




    public class tblAdminUser
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(500)]

        public string FirstName { get; set; }


        [MaxLength(500)]
        public string LastName { get; set; }

        [MaxLength(500)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string MobileNo { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public bool Status { get; set; }

        public int? UserGroupId { get; set; }

        public byte? UserGroupTypeId { get; set; }

        [MaxLength(50)]
        public string IqaamaNo { get; set; }

        public int AccountTypeId { get; set; }

        [MaxLength(50)]
        public string ProfilePic { get; set; }

        public DateTime AddedDate { get; set; }
        public int AddedBy { get; set; }

        public virtual tblUserGroupCompanies UserGroups { get; set; } //Fk table

        public tblAdminUser()
        {
            this.AdminServices = new HashSet<tblServices>();
            this.AdminLocations = new HashSet<tblLocations>();
            this.AdminHistory = new HashSet<tblOrderHistory>();
        }

        public virtual ICollection<tblServices> AdminServices { get; set; }
        public virtual ICollection<tblLocations> AdminLocations { get; set; }
        public virtual ICollection<tblOrderHistory> AdminHistory { get; set; }
    }

    public class tblUserGroupCompanies
    {
        public tblUserGroupCompanies() //primary tbl
        {
            this.AdminUsers = new HashSet<tblAdminUser>();
        }

        [Key]
        public int UserGroupId { get; set; }

        [Required]
        [MaxLength(500)]
        public string CompanyNameEN { get; set; }

        [Required]
        [MaxLength(500)]
        public string CompanyNameAR { get; set; }

        [Required]
        public byte UserGroupTypeId { get; set; }

        [MaxLength(50)]
        public string Telephone { get; set; }

        [MaxLength(50)]
        public string Fax { get; set; }

        [MaxLength(500)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string CompanyLogo { get; set; }

        public bool Status { get; set; }

        public DateTime AddedDate { get; set; }

        public int AddedBy { get; set; }
        public virtual ICollection<tblAdminUser> AdminUsers { get; set; }
    }

    public class tblServices
    {
        [Key]
        public int ServiceId { get; set; }

        [MaxLength(500)]
        public string ServiceNameEN { get; set; }
        [MaxLength(500)]
        public string ServiceNameAR { get; set; }
        public decimal UnitPrice { get; set; }
        public bool Status { get; set; }
        public DateTime AddedDate { get; set; }

        public int UserId { get; set; }

        public virtual tblAdminUser UserAdmins { get; set; }

        public tblServices()
        {
            this.OrderServices = new HashSet<tblOrderServices>();
        }

        public virtual ICollection<tblOrderServices> OrderServices { get; set; }
    }

    public class tblLocations
    {
        [Key]
        public int LocationId { get; set; }
        [MaxLength(500)]
        public string LocationNameEN { get; set; }
        [MaxLength(500)]
        public string LocationNameAR { get; set; }
        public bool Status { get; set; }
        public DateTime AddedDate { get; set; }

        public int UserId { get; set; }

        public virtual tblAdminUser UserAdmins { get; set; }

        public tblLocations()
        {
            this.OrderLocations = new HashSet<tblOrder>();
        }

        public virtual ICollection<tblOrder> OrderLocations { get; set; }

    }

    public class tblSetting
    {
        [Key]
        public int Id { get; set; }
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
    }

    public class tblSMS
    {
        [Key]
        public int Id { get; set; }
        public string KeyName { get; set; }
        public string SMSTextEN { get; set; }
        public string SMSTextAR { get; set; }
    }
}
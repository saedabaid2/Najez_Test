//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Almanea
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblUserGroupCompany
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblUserGroupCompany()
        {
            this.tblLaborInactives = new HashSet<tblLaborInactive>();
            this.tblProviderTimeSlots = new HashSet<tblProviderTimeSlot>();
            this.tblProviderWorkinHours = new HashSet<tblProviderWorkinHour>();
            this.tblTeamCapacities = new HashSet<tblTeamCapacity>();
            this.tblTeamCapacityCalculations = new HashSet<tblTeamCapacityCalculation>();
            this.tblAdminUsers = new HashSet<tblAdminUser>();
        }
    
        public int UserGroupId { get; set; }
        public string CompanyNameEN { get; set; }
        public string CompanyNameAR { get; set; }
        public byte UserGroupTypeId { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string CompanyLogo { get; set; }
        public Nullable<decimal> Contract { get; set; }
        public bool Status { get; set; }
        public System.DateTime AddedDate { get; set; }
        public int AddedBy { get; set; }
        public bool IsInternal { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblLaborInactive> tblLaborInactives { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblProviderTimeSlot> tblProviderTimeSlots { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblProviderWorkinHour> tblProviderWorkinHours { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblTeamCapacity> tblTeamCapacities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblTeamCapacityCalculation> tblTeamCapacityCalculations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblAdminUser> tblAdminUsers { get; set; }
    }
}

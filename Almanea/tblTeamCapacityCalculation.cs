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
    
    public partial class tblTeamCapacityCalculation
    {
        public int Id { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<System.DateTime> InstallDate { get; set; }
        public Nullable<int> ServiceProviderId { get; set; }
        public Nullable<int> DailyCapacity { get; set; }
        public Nullable<int> ConsumedCapacity { get; set; }
        public Nullable<int> CurrentCapacity { get; set; }
        public Nullable<decimal> CapcityPercentage { get; set; }
        public Nullable<System.DateTime> Updatedate { get; set; }
        public tblProviderTimeSlot tblProviderTimeSlot { get; set; }
        public virtual tblOrder tblOrder { get; set; }
        public virtual tblUserGroupCompany tblUserGroupCompany { get; set; }
    }
}

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
    
    public partial class OrdersAssigned
    {
        public int Id { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> LabourId { get; set; }
        public Nullable<int> DriverId { get; set; }
        public Nullable<int> ServiceId { get; set; }
        public Nullable<int> Status { get; set; }
        public string Quantity { get; set; }
        public Nullable<System.DateTime> Created_At { get; set; }
        public string Isleader { get; set; }
        public Nullable<int> Total { get; set; }
        public string DeliveryStatus { get; set; }
        public string WhyPartial { get; set; }
        public Nullable<int> AdditionalWorkOtp { get; set; }
    
        public virtual tblOrder tblOrder { get; set; }
        public virtual tblService tblService { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class ProviderWorkinHourDto
    {
        public int Id { get; set; }
        public Nullable<int> ServiceProviderId { get; set; }
        public Nullable<int> WorkingHours { get; set; }
        public string CompanyNameEN { get; set; }
        
    }

    public  class vm_TeamCapacity
    {
        public int Id { get; set; }
        public Nullable<int> ServiceProviderId { get; set; }
        public Nullable<int> DailyCapacity { get; set; }
        public Nullable<int> ConsumedCapacity { get; set; }
        public Nullable<System.DateTime> Updatedate { get; set; }
        public Nullable<int> CurrentCapacity { get; set; }

        public string tblUserGroupCompany { get; set; }
    }
    public class vm_tblTeamCapacity
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

        public string tblUserGroupCompany { get; set; }
        public string tblOrder { get; set; }
    }
}
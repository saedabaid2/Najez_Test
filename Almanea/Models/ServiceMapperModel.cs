using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class ServiceMapperModel
    {
        public class vm_ServicesMapper
        {
            public int ServiceId { get; set; }
            public string ServiceProviderId { get; set; }
            public string SupplierId { get; set; }
            public bool CreatedOn { get; set; }
            public string UserId { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class vm_Output
    {
    }


    public class vm_jsOutput
    {
        public int StatusId { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string Data2 { get; set; }

    }

    public class vm_Result
    {
        public int Count { get; set; }
        public object Data { get; set; }
    }
    public class vm_StatusInfo
    {
        public string Status { get; set; }
        public int? StatusCode { get; set; }
        public string Message { get; set; }
    }


}
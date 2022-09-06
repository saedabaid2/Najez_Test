using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class vm_page_details
    {
        public int Id { get; set; }
        public Nullable<int> Page_Id { get; set; }
        public Nullable<int> FieldType { get; set; }
        public string FieldName { get; set; }
        public Nullable<int> SP { get; set; }
        public Nullable<int> Agent { get; set; }
        public Nullable<int> Supplier { get; set; }
        public Nullable<int> Display { get; set; }
        public Nullable<int> Target { get; set; }
        //0==>sp 1==>agent 2==>supplier
        public virtual Admin_Pages Admin_Pages { get; set; }
    }
}


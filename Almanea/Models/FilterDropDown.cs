using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class FilterDropDown
    {
        public int SupplierId { get; set; }
        public int StatusId { get; set; }
        public int LocationId { get; set; }
        public string Date { get; set; }
    }
}
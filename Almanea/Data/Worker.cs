using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Almanea.Data;
using System.Data.Entity;

namespace Almanea
{
    public class Worker : cls_Dispose
    {
        private static AlmaneaDbEntities _context;

        public string uiCulture = CultureInfo.CurrentCulture.Name;
        public static bool isEnglish = (CultureInfo.CurrentCulture.Name.Equals("ar")) ? false : true;

        public Worker()
        {
            _context = new AlmaneaDbEntities();
            isEnglish = (CultureInfo.CurrentCulture.Name.Equals(HttpContext.Current.Session["S_Culture"].ToString())) ? false : true;
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public string FixTrnx(string TitleEN, string TitleAR)
        {
            return (isEnglish ? TitleEN : TitleAR);
        }

        public static string GetCompanyName(int? Id)
        {
            var model = _context.tblUserGroupCompanies.Where(x => x.UserGroupId == Id).FirstOrDefault();
            if (model != null)
                return (isEnglish ? model.CompanyNameEN : model.CompanyNameAR);

            return "";
        }

        public string GetLocationName(int? Id)
        {
            var model = _context.tblLocations.Where(x => x.LocationId == Id).FirstOrDefault();
            if (model != null)
                return (isEnglish ? model.LocationNameEN : model.LocationNameAR);

            return "";
        }

        public string GetServiceName(int? Id)
        {
            var model = _context.tblServices.Where(x => x.ServiceId == Id).FirstOrDefault();
            if (model != null)
                return (isEnglish ? model.ServiceNameEN : model.ServiceNameAR);

            return "";
        }

        public string GetComplainType(int? Id)
        {
            var model = _context.tblComplainTypes.Where(x => x.ComplainTypeId == Id).FirstOrDefault();
            if (model != null)
                return (isEnglish ? model.TitleEN : model.TitleAR);

            return "";
        }
    }
}
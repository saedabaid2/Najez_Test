using Almanea.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Almanea
{
    public class BaseController : Controller
    {
        public bool IsEnglish = (CultureInfo.CurrentCulture.Name.Contains("en")) ? true : false;

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            //Session
            var chkTimeOut = Session.Timeout;
            if (chkTimeOut < 25)
            {
                // set new time out to session  
                Session.Timeout = 60;
            }

            try
            {
                string cultureName = null;

                // Attempt to read the culture cookie from Request
                HttpCookie cultureCookie = Request.Cookies["Syanah_culture"];
                if (cultureCookie != null)
                    cultureName = cultureCookie.Value;
                else
                    cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                            Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
                            null;
                // Validate culture name
                cultureName = cultureHelper.GetImplementedCulture(cultureName); // This is safe

                // Modify current thread's cultures            
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
                DateTimeFormatInfo englishDateTimeFormat = new CultureInfo("en-US").DateTimeFormat;                englishDateTimeFormat.ShortDatePattern = "dd/MM/yyyy";                englishDateTimeFormat.DateSeparator = "/";                Thread.CurrentThread.CurrentCulture.DateTimeFormat = englishDateTimeFormat;

                IsEnglish = (cultureName.Equals("ar") ? false : true);
                cls_Defaults.IsEnglish = IsEnglish;
                Session["S_Culture"] = IsEnglish;
            }
            catch (Exception ex) { }

            return base.BeginExecuteCore(callback, state);
        }
    }
}
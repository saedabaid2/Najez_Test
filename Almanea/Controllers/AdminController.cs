using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Almanea.Data;

namespace Almanea.Controllers
{
    public class AdminController : BaseController
    {
        private db_Settings objSettings = new db_Settings();
        // GET: Admin
        public ActionResult Index()
        {
            var model = objSettings.GetAdminPage();

            return View(model);
        }
    }
}
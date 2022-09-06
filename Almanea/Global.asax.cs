using Almanea.App_Start;
using Almanea.BusinessLogic;
using Almanea.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Almanea
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AutoMapConfig.configure();

            // Initialize the product database.
            //Database.SetInitializer(new DBIntializer());

            //DataContext con = new DataContext();
            //con.Database.Initialize(true);
            //con.Database.CreateIfNotExists();
        }
    }
}

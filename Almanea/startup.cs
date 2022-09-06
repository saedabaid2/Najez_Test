using Almanea.BusinessLogic;
using Almanea.Data;
using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(Almanea.startup))]
namespace Almanea
{
    public class startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
               .UseSqlServerStorage("dbConn");

            //BackgroundJob.Schedule(() => ScheduleJobs.SendSMS(), TimeSpan.FromMinutes(1));
            //RecurringJob.AddOrUpdate(() => ScheduleJobs.SendSMS(), "*/5 * * * *");

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });

            app.UseHangfireDashboard();
            app.UseHangfireServer();
                        
        }
    }
}
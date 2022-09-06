using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Almanea.BusinessLogic;
using Almanea.Data;
using Hangfire;

namespace Almanea
{
    public static class ScheduleJobs
    {

        public static async Task SendSMS()
        {
            db_User objUser = new db_User();
            var admin = await objUser.GetUserByTypeId((int)enumGroupType.Admin);

            //SMS To Admin 
            var objSettings = new db_Settings();

            var setting = await objSettings.GetSetting();

            var duration = Convert.ToInt32(setting.OrderDuration);

            var orders = await objSettings.ExpiredOrders(duration);

            foreach (var a in orders)
            {
                await cls_Sms.ExpireOrder(a.Value);

                await objSettings.UpdateReportAdmin(a.Key);
            }
               

        }
    }
}
using Almanea.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Almanea.Data
{
    public class DBIntializer : DropCreateDatabaseIfModelChanges<DataContext>
    {
        protected override void Seed(DataContext context)
        {

            if (!context.tblAdminUsers.Any())
            {
                var user = db_User.FirstUser();

                context.tblAdminUsers.Add(user);
            }

            if (!context.tblSMS.Any())
            {
                IList<tblSMS> sms = new List<tblSMS>();
                sms.Add(new tblSMS() { KeyName = "NEWORDER", SMSTextEN = "There is a new order {ORDERNO} has been submitted, please check it now and take action accordingly.", SMSTextAR = "There is a new order {ORDERNO} has been submitted, please check it now and take action accordingly." });
                sms.Add(new tblSMS() { KeyName = "CUSTOMERCONFIRM", SMSTextEN = "Dear customer, Your order schedule is confirmed on {INSTALLDATE}. Customer confirmation code is {CODE} to ensure your satisfaction about the service.", SMSTextAR = "Dear customer, Your order schedule is confirmed on {INSTALLDATE}. Customer confirmation code is {CODE} to ensure your satisfaction about the service." });
                sms.Add(new tblSMS() { KeyName = "NOACTION", SMSTextEN = "Order {ORDERNO} has not taken any action from service providers.", SMSTextAR = "Order {ORDERNO} has not taken any action from service providers." });
                sms.Add(new tblSMS() { KeyName = "FORGOT", SMSTextEN = "Your password is : {PASSWORD}", SMSTextAR = "Your password is : {PASSWORD}" });

                context.tblSMS.AddRange(sms);
            }

            if (!context.tblSetting.Any(x => x.KeyName.ToLower().Equals("vat")))
            {
                IList<tblSetting> Setting = new List<tblSetting>();
                Setting.Add(new tblSetting() { KeyName = "Vat", KeyValue = "5" });
                Setting.Add(new tblSetting() { KeyName = "OrderDuration", KeyValue = "30" });

                context.tblSetting.AddRange(Setting);
            }

            base.Seed(context);
        }
    }

}
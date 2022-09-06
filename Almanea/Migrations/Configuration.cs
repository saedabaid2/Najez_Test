namespace Almanea.Migrations
{
    using Almanea.BusinessLogic;
    using Almanea.Data;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Almanea.Data.DataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Almanea.Data.DataContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            //Create new User
            if (!context.tblAdminUsers.Any())
            {
                var user = db_User.FirstUser();

                context.tblAdminUsers.Add(user);
            }

            if (!context.tblSMS.Any())
            {
                IList<tblSMS> sms = new List<tblSMS>();
                sms.Add(new tblSMS() { KeyName = "NEWORDER", SMSTextEN = "There is a new order {OrderNo} has been submitted, please check it now and take action accordingly.", SMSTextAR = "There is a new order {OrderNo} has been submitted, please check it now and take action accordingly." });
                sms.Add(new tblSMS() { KeyName = "CUSTOMERCONFIRM", SMSTextEN = "Dear customer, Your order schedule is confirmed on {InstallDate}. Customer confirmation code is {Code} to ensure your satisfaction about the service", SMSTextAR = "Dear customer, Your order schedule is confirmed on {InstallDate}. Customer confirmation code is {Code} to ensure your satisfaction about the service" });
                sms.Add(new tblSMS() { KeyName = "NOACTION", SMSTextEN = "Order {OrderNo} has not taken any action from service providers.", SMSTextAR = "Order {OrderNo} has not taken any action from service providers." });

                context.tblSMS.AddRange(sms);
            }

            if (!context.tblSetting.Any(x => x.KeyName.ToLower().Equals("vat")))
            {
                IList<tblSetting> Setting = new List<tblSetting>();
                Setting.Add(new tblSetting() { KeyName = "Vat", KeyValue = "5" });

                context.tblSetting.AddRange(Setting);
            }
            
        }
    }
}

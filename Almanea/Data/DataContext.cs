
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Almanea.Data
{
    public class DataContext : DbContext
    {
        public DataContext() : base("dbConn")
        {

            //Database.SetInitializer<DataContext>(new CreateDatabaseIfNotExists<DataContext>());

            //Database.SetInitializer(new DBIntializer());
        }

        public DbSet<tblAdminUser> tblAdminUsers { get; set; } // My domain models
        public DbSet<tblUserGroupCompanies> tblUserGroupCompanies { get; set; }// My domain models

        public DbSet<tblServices> tblServices { get; set; }// My domain models

        public DbSet<tblLocations> tblLocations { get; set; }// My domain models

        public DbSet<tblOrder> tblOrders { get; set; }// My domain models

        public DbSet<tblOrderServices> tblOrderServices { get; set; }// My domain models

        public DbSet<tblOrderHistory> tblOrderHistory { get; set; }// My domain models

        public DbSet<tblAdditionalServices> tblAdditionalServices { get; set; }// My domain models

        public DbSet<tblSetting> tblSetting { get; set; }// My domain models
        public DbSet<tblSMS> tblSMS { get; set; }// My domain models

        public DbSet<tblOrderRelease> tblOrderRelease { get; set; }
    }


}
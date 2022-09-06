namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOrder : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblOrders", "InstallDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblOrders", "InstallDate", c => c.DateTime(nullable: false));
        }
    }
}

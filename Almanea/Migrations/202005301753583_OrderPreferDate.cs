namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderPreferDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrders", "PreferDate", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblOrders", "PreferDate");
        }
    }
}

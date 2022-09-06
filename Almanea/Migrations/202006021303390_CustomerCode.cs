namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrders", "PrefferHr", c => c.Int());
            AddColumn("dbo.tblOrders", "PrefferMeridian", c => c.Byte());
            AddColumn("dbo.tblOrders", "CustomerCode", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblOrders", "CustomerCode");
            DropColumn("dbo.tblOrders", "PrefferMeridian");
            DropColumn("dbo.tblOrders", "PrefferHr");
        }
    }
}

namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditOrderRestrict : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrders", "IsOnEdit", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblOrders", "IsOnEdit");
        }
    }
}

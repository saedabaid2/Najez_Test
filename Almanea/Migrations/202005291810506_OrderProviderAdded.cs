namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderProviderAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrders", "ReservedProvider", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblOrders", "ReservedProvider");
        }
    }
}

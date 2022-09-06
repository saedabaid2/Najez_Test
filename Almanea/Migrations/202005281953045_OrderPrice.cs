namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrders", "Vat", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tblOrders", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblOrders", "TotalAmount");
            DropColumn("dbo.tblOrders", "Vat");
        }
    }
}

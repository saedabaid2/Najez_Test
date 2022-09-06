namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalServices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblAdditionalServices",
                c => new
                    {
                        AdditionalServiceId = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        ServiceName = c.String(),
                        Quantity = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SpareParts = c.String(),
                        AddedOn = c.DateTime(nullable: false, defaultValue: DateTime.Now),
                        Status = c.Boolean(nullable: false, defaultValue: true),
                    })
                .PrimaryKey(t => t.AdditionalServiceId)
                .ForeignKey("dbo.tblOrders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            AddColumn("dbo.tblOrderHistories", "FileAttachment", c => c.String());


        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblAdditionalServices", "OrderId", "dbo.tblOrders");
            DropIndex("dbo.tblAdditionalServices", new[] { "OrderId" });
            DropColumn("dbo.tblOrderHistories", "FileAttachment");
            DropTable("dbo.tblAdditionalServices");
        }
    }
}

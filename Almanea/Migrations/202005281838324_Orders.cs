namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Orders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblOrders",
                c => new
                {
                    OrderId = c.Int(nullable: false, identity: true),
                    SellerName = c.String(maxLength: 500),
                    SellerContact = c.String(maxLength: 50),
                    InvoiceNo = c.String(maxLength: 150),
                    CustomerName = c.String(maxLength: 500),
                    CustomerContact = c.String(maxLength: 50),
                    LocationId = c.Int(nullable: false),
                    InstallDate = c.DateTime(nullable: false),
                    PrefferTime = c.Byte(nullable: false),
                    Status = c.Byte(nullable: false),
                    AddedDate = c.DateTime(nullable: false),
                    SupplierId = c.Int(nullable: false),
                    AddedBy = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.tblLocations", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.LocationId);

            CreateTable(
                "dbo.tblOrderServices",
                c => new
                {
                    OrderServiceId = c.Int(nullable: false, identity: true),
                    OrderId = c.Int(nullable: false),
                    ServiceId = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.OrderServiceId)
                .ForeignKey("dbo.tblOrders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.tblServices", t => t.ServiceId, cascadeDelete: false)
                .Index(t => t.OrderId)
                .Index(t => t.ServiceId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.tblOrderServices", "ServiceId", "dbo.tblServices");
            DropForeignKey("dbo.tblOrderServices", "OrderId", "dbo.tblOrders");
            DropForeignKey("dbo.tblOrders", "LocationId", "dbo.tblLocations");
            DropIndex("dbo.tblOrderServices", new[] { "ServiceId" });
            DropIndex("dbo.tblOrderServices", new[] { "OrderId" });
            DropIndex("dbo.tblOrders", new[] { "LocationId" });
            DropTable("dbo.tblOrderServices");
            DropTable("dbo.tblOrders");
        }
    }
}

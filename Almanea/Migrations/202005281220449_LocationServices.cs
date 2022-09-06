namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class LocationServices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblLocations",
                c => new
                {
                    LocationId = c.Int(nullable: false, identity: true),
                    LocationNameEN = c.String(maxLength: 500),
                    LocationNameAR = c.String(maxLength: 500),
                    Status = c.Boolean(nullable: false, defaultValue: true),
                    AddedDate = c.DateTime(nullable: false, defaultValue: DateTime.Now),
                    UserId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.LocationId)
                .ForeignKey("dbo.tblAdminUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.tblServices",
                c => new
                {
                    ServiceId = c.Int(nullable: false, identity: true),
                    ServiceNameEN = c.String(maxLength: 500),
                    ServiceNameAR = c.String(maxLength: 500),
                    UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Boolean(nullable: false, defaultValue: true),
                    AddedDate = c.DateTime(nullable: false, defaultValue: DateTime.Now),
                    UserId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ServiceId)
                .ForeignKey("dbo.tblAdminUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.tblServices", "UserId", "dbo.tblAdminUsers");
            DropForeignKey("dbo.tblLocations", "UserId", "dbo.tblAdminUsers");
            DropIndex("dbo.tblServices", new[] { "UserId" });
            DropIndex("dbo.tblLocations", new[] { "UserId" });
            DropTable("dbo.tblServices");
            DropTable("dbo.tblLocations");
        }
    }
}

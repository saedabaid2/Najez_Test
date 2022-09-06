namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblOrderHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ActivityDate = c.DateTime(nullable: false, defaultValue: DateTime.Now),
                        Status = c.Byte(nullable: false),
                        UserId = c.Int(nullable: false),
                        Comments = c.String(maxLength: 2500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.tblAdminUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.tblOrders", t => t.OrderId, cascadeDelete: false)
                .Index(t => t.OrderId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblOrderHistories", "OrderId", "dbo.tblOrders");
            DropForeignKey("dbo.tblOrderHistories", "UserId", "dbo.tblAdminUsers");
            DropIndex("dbo.tblOrderHistories", new[] { "UserId" });
            DropIndex("dbo.tblOrderHistories", new[] { "OrderId" });
            DropTable("dbo.tblOrderHistories");
        }
    }
}

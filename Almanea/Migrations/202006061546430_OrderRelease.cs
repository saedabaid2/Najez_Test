namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderRelease : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblOrderReleases",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ActivityDate = c.DateTime(nullable: false),
                        Status = c.Byte(nullable: false),
                        UserId = c.Int(nullable: false),
                        Comments = c.String(maxLength: 2500),
                        FileAttachment = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblOrderReleases");
        }
    }
}

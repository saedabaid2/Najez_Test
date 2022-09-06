namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SMS : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        KeyName = c.String(),
                        KeyValue = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.tblSMS",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        KeyName = c.String(),
                        SMSTextEN = c.String(),
                        SMSTextAR = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblSMS");
            DropTable("dbo.tblSettings");
        }
    }
}

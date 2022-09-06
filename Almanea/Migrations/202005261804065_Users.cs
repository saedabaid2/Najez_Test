namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Users : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblAdminUsers",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 500),
                        LastName = c.String(nullable: false, maxLength: 500),
                        Email = c.String(maxLength: 500),
                        MobileNo = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.Binary(),
                        PasswordSalt = c.Binary(),
                        Status = c.Boolean(nullable: false),
                        UserGroupId = c.Int(nullable: false),
                        UserGroupTypeId = c.Byte(),
                        IqaamaNo = c.String(maxLength: 50),
                        AccountTypeId = c.Int(nullable: false),
                        ProfilePic = c.String(maxLength: 50),
                        AddedDate = c.DateTime(nullable: false),
                        AddedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.tblUserGroupCompanies", t => t.UserGroupId, cascadeDelete: true)
                .Index(t => t.UserGroupId);
            
            CreateTable(
                "dbo.tblUserGroupCompanies",
                c => new
                    {
                        UserGroupId = c.Int(nullable: false, identity: true),
                        CompanyNameEN = c.String(nullable: false, maxLength: 500),
                        CompanyNameAR = c.String(nullable: false, maxLength: 500),
                        UserGroupTypeId = c.Byte(nullable: false),
                        Telephone = c.String(maxLength: 50),
                        Fax = c.String(maxLength: 50),
                        Email = c.String(maxLength: 500),
                        CompanyLogo = c.String(maxLength: 50),
                        Status = c.Boolean(nullable: false),
                        AddedDate = c.DateTime(nullable: false),
                        AddedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserGroupId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblAdminUsers", "UserGroupId", "dbo.tblUserGroupCompanies");
            DropIndex("dbo.tblAdminUsers", new[] { "UserGroupId" });
            DropTable("dbo.tblUserGroupCompanies");
            DropTable("dbo.tblAdminUsers");
        }
    }
}

namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateUser : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblAdminUsers", "UserGroupId", "dbo.tblUserGroupCompanies");
            DropIndex("dbo.tblAdminUsers", new[] { "UserGroupId" });
            AlterColumn("dbo.tblAdminUsers", "FirstName", c => c.String(maxLength: 500));
            AlterColumn("dbo.tblAdminUsers", "LastName", c => c.String(maxLength: 500));
            AlterColumn("dbo.tblAdminUsers", "MobileNo", c => c.String(maxLength: 50));
            AlterColumn("dbo.tblAdminUsers", "UserGroupId", c => c.Int());
            CreateIndex("dbo.tblAdminUsers", "UserGroupId");
            AddForeignKey("dbo.tblAdminUsers", "UserGroupId", "dbo.tblUserGroupCompanies", "UserGroupId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblAdminUsers", "UserGroupId", "dbo.tblUserGroupCompanies");
            DropIndex("dbo.tblAdminUsers", new[] { "UserGroupId" });
            AlterColumn("dbo.tblAdminUsers", "UserGroupId", c => c.Int(nullable: false));
            AlterColumn("dbo.tblAdminUsers", "MobileNo", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.tblAdminUsers", "LastName", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.tblAdminUsers", "FirstName", c => c.String(nullable: false, maxLength: 500));
            CreateIndex("dbo.tblAdminUsers", "UserGroupId");
            AddForeignKey("dbo.tblAdminUsers", "UserGroupId", "dbo.tblUserGroupCompanies", "UserGroupId", cascadeDelete: true);
        }
    }
}

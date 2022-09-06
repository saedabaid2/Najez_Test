namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class OrderServiceAdditional : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrderServices", "IsAdditional", c => c.Int(defaultValue: 0));
        }

        public override void Down()
        {
            DropColumn("dbo.tblOrderServices", "IsAdditional");
        }
    }
}

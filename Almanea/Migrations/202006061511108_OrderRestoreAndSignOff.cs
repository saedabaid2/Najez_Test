namespace Almanea.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderRestoreAndSignOff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblOrders", "CustomerSignOff", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblOrders", "CustomerSignOff");
        }
    }
}

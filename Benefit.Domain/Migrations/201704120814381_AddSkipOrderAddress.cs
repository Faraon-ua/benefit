namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSkipOrderAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShippingMethods", "SkipOrderAddress", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShippingMethods", "SkipOrderAddress");
        }
    }
}

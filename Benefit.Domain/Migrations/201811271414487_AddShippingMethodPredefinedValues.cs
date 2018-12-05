namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingMethodPredefinedValues : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShippingMethods", "Description", c => c.String());
            AddColumn("dbo.ShippingMethods", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShippingMethods", "Type");
            DropColumn("dbo.ShippingMethods", "Description");
        }
    }
}

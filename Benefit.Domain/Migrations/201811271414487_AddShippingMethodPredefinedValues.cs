namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingMethodPredefinedValues : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ApplicationUsers", new[] { "PhoneNumber" });
            AddColumn("dbo.ShippingMethods", "Description", c => c.String());
            AddColumn("dbo.ShippingMethods", "Type", c => c.Int(nullable: false));
            AlterColumn("dbo.Addresses", "Phone", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.ApplicationUsers", "PhoneNumber", c => c.String(maxLength: 20));
            CreateIndex("dbo.ApplicationUsers", "PhoneNumber", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.ApplicationUsers", new[] { "PhoneNumber" });
            AlterColumn("dbo.ApplicationUsers", "PhoneNumber", c => c.String(maxLength: 16));
            AlterColumn("dbo.Addresses", "Phone", c => c.String(nullable: false, maxLength: 16));
            DropColumn("dbo.ShippingMethods", "Type");
            DropColumn("dbo.ShippingMethods", "Description");
            CreateIndex("dbo.ApplicationUsers", "PhoneNumber", unique: true);
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductWeightProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "IsWeightProduct", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "AvailableAmount", c => c.Int());
            AddColumn("dbo.Products", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.Products", "DoesCountForShipping", c => c.Boolean(nullable: false));
            DropColumn("dbo.Products", "Amount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Amount", c => c.Int());
            DropColumn("dbo.Products", "DoesCountForShipping");
            DropColumn("dbo.Products", "Order");
            DropColumn("dbo.Products", "AvailableAmount");
            DropColumn("dbo.Products", "IsWeightProduct");
        }
    }
}

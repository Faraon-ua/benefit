namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomMarginToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "CustomMargin", c => c.Int());
            DropColumn("dbo.SellerCategories", "CustomMargin");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SellerCategories", "CustomMargin", c => c.Int());
            DropColumn("dbo.Products", "CustomMargin");
        }
    }
}

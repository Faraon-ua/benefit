namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderAndBindToProductOoption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductOptions", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.ProductOptions", "BindedProductOptionId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ProductOptions", "BindedProductOptionId");
            AddForeignKey("dbo.ProductOptions", "BindedProductOptionId", "dbo.ProductOptions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductOptions", "BindedProductOptionId", "dbo.ProductOptions");
            DropIndex("dbo.ProductOptions", new[] { "BindedProductOptionId" });
            DropColumn("dbo.ProductOptions", "BindedProductOptionId");
            DropColumn("dbo.ProductOptions", "Order");
        }
    }
}

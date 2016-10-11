namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddProductOptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductOptions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 64),
                        MultipleSelection = c.Boolean(nullable: false),
                        PriceGrowth = c.Double(nullable: false),
                        ParentProductOptionId = c.String(maxLength: 128),
                        ProductId = c.String(maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.ProductOptions", t => t.ParentProductOptionId)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.ParentProductOptionId)
                .Index(t => t.ProductId)
                .Index(t => t.CategoryId)
                .Index(t => t.SellerId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductOptions", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.ProductOptions", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductOptions", "ParentProductOptionId", "dbo.ProductOptions");
            DropForeignKey("dbo.ProductOptions", "CategoryId", "dbo.Categories");
            DropIndex("dbo.ProductOptions", new[] { "SellerId" });
            DropIndex("dbo.ProductOptions", new[] { "CategoryId" });
            DropIndex("dbo.ProductOptions", new[] { "ProductId" });
            DropIndex("dbo.ProductOptions", new[] { "ParentProductOptionId" });
            DropTable("dbo.ProductOptions");
        }
    }
}

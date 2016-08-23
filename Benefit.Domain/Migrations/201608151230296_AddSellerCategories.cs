namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerCategories : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 64),
                        UrlName = c.String(nullable: false, maxLength: 128),
                        NavigationType = c.String(nullable: false, maxLength: 32),
                        Description = c.String(nullable: false, maxLength: 256),
                        ImageUrl = c.String(),
                        Order = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        LastModified = c.DateTime(nullable: false),
                        LastModifiedBy = c.String(),
                        ParentCategoryId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.ParentCategoryId)
                .Index(t => t.ParentCategoryId);
            
            CreateTable(
                "dbo.SellerCategories",
                c => new
                    {
                        SellerId = c.String(nullable: false, maxLength: 128),
                        CategoryId = c.String(nullable: false, maxLength: 128),
                        CustomDiscount = c.Double(),
                    })
                .PrimaryKey(t => new { t.SellerId, t.CategoryId })
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Sellers", t => t.SellerId, cascadeDelete: true)
                .Index(t => t.SellerId)
                .Index(t => t.CategoryId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SellerCategories", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.SellerCategories", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Categories", "ParentCategoryId", "dbo.Categories");
            DropIndex("dbo.SellerCategories", new[] { "CategoryId" });
            DropIndex("dbo.SellerCategories", new[] { "SellerId" });
            DropIndex("dbo.Categories", new[] { "ParentCategoryId" });
            DropTable("dbo.SellerCategories");
            DropTable("dbo.Categories");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExportProducts : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExportImports", "SellerId", "dbo.Sellers");
            DropIndex("dbo.ExportImports", new[] { "SellerId" });
            CreateTable(
                "dbo.ExportCategories",
                c => new
                    {
                        CategoryId = c.String(nullable: false, maxLength: 128),
                        ExportId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 64),
                    })
                .PrimaryKey(t => new { t.CategoryId, t.ExportId })
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.ExportImports", t => t.ExportId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.ExportId);
            
            CreateTable(
                "dbo.ExportProducts",
                c => new
                    {
                        ProductId = c.String(nullable: false, maxLength: 128),
                        ExportId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ProductId, t.ExportId })
                .ForeignKey("dbo.ExportImports", t => t.ExportId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.ExportId);
            
            AddColumn("dbo.ExportImports", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.ExportImports", "FileUrl", c => c.String(maxLength: 400));
            AlterColumn("dbo.ExportImports", "SellerId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ExportImports", "SellerId");
            AddForeignKey("dbo.ExportImports", "SellerId", "dbo.Sellers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExportImports", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.ExportCategories", "ExportId", "dbo.ExportImports");
            DropForeignKey("dbo.ExportProducts", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ExportProducts", "ExportId", "dbo.ExportImports");
            DropForeignKey("dbo.ExportCategories", "CategoryId", "dbo.Categories");
            DropIndex("dbo.ExportProducts", new[] { "ExportId" });
            DropIndex("dbo.ExportProducts", new[] { "ProductId" });
            DropIndex("dbo.ExportImports", new[] { "SellerId" });
            DropIndex("dbo.ExportCategories", new[] { "ExportId" });
            DropIndex("dbo.ExportCategories", new[] { "CategoryId" });
            AlterColumn("dbo.ExportImports", "SellerId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.ExportImports", "FileUrl", c => c.String());
            DropColumn("dbo.ExportImports", "Name");
            DropTable("dbo.ExportProducts");
            DropTable("dbo.ExportCategories");
            CreateIndex("dbo.ExportImports", "SellerId");
            AddForeignKey("dbo.ExportImports", "SellerId", "dbo.Sellers", "Id", cascadeDelete: true);
        }
    }
}

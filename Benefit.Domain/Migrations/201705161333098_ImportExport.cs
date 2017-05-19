namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImportExport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExportImports",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsImport = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        RemoveProducts = c.Boolean(nullable: false),
                        SyncType = c.Int(nullable: false),
                        FileUrl = c.String(),
                        SyncPeriod = c.Int(nullable: false),
                        LastUpdateStatus = c.Boolean(),
                        LastUpdateMessage = c.String(),
                        LastSync = c.DateTime(storeType: "datetime2"),                        
                        ProductsAdded = c.Int(),
                        ProductsModified = c.Int(),
                        ProductsRemoved = c.Int(),
                        SellerId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId, cascadeDelete: true)
                .Index(t => t.SellerId);
            
            AddColumn("dbo.Categories", "IsSellerCategory", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "SellerId", c => c.String(maxLength: 128));
            AddColumn("dbo.Categories", "MappedParentCategoryId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Categories", "SellerId");
            CreateIndex("dbo.Categories", "MappedParentCategoryId");
            AddForeignKey("dbo.Categories", "MappedParentCategoryId", "dbo.Categories", "Id");
            AddForeignKey("dbo.Categories", "SellerId", "dbo.Sellers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExportImports", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Categories", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Categories", "MappedParentCategoryId", "dbo.Categories");
            DropIndex("dbo.ExportImports", new[] { "SellerId" });
            DropIndex("dbo.Categories", new[] { "MappedParentCategoryId" });
            DropIndex("dbo.Categories", new[] { "SellerId" });
            DropColumn("dbo.Categories", "MappedParentCategoryId");
            DropColumn("dbo.Categories", "SellerId");
            DropColumn("dbo.Categories", "IsSellerCategory");
            DropTable("dbo.ExportImports");
        }
    }
}

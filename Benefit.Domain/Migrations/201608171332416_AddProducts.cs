namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddProducts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        UrlName = c.String(nullable: false, maxLength: 128),
                        SKU = c.Int(nullable: false),
                        Description = c.String(nullable: false),
                        Price = c.Double(nullable: false),
                        Amount = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        LastModified = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastModifiedBy = c.String(maxLength: 64),
                        CategoryId = c.String(nullable: false, maxLength: 128),
                        SellerId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Sellers", t => t.SellerId, cascadeDelete: true)
                .Index(t => t.Name)
                .Index(t => t.SKU)
                .Index(t => t.CategoryId)
                .Index(t => t.SellerId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Products", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "SellerId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.Products", new[] { "SKU" });
            DropIndex("dbo.Products", new[] { "Name" });
            DropTable("dbo.Products");
        }
    }
}

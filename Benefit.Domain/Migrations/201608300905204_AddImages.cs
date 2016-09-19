namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddImages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ImageUrl = c.String(nullable: false, maxLength: 256),
                        ImageType = c.Int(nullable: false),
                        SellerId = c.String(maxLength: 128),
                        ProductId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId)
                .Index(t => t.ProductId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Images", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Images", "ProductId", "dbo.Products");
            DropIndex("dbo.Images", new[] { "ProductId" });
            DropIndex("dbo.Images", new[] { "SellerId" });
            DropTable("dbo.Images");
        }
    }
}

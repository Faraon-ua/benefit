namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddPromotions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Promotions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Start = c.DateTime(nullable: false, storeType: "datetime2"),
                        End = c.DateTime(nullable: false, storeType: "datetime2"),
                        ProductId = c.String(maxLength: 128),
                        SellerId = c.String(maxLength: 128),
                        InstantDiscountFrom = c.Double(nullable: false),
                        InstantDiscountValue = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.ProductId)
                .Index(t => t.SellerId);
            
            AlterColumn("dbo.Reviews", "Rating", c => c.Int());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Promotions", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Promotions", "ProductId", "dbo.Products");
            DropIndex("dbo.Promotions", new[] { "SellerId" });
            DropIndex("dbo.Promotions", new[] { "ProductId" });
            AlterColumn("dbo.Reviews", "Rating", c => c.Int(nullable: false));
            DropTable("dbo.Promotions");
        }
    }
}

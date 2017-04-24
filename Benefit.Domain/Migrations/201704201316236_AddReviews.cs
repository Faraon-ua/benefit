namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReviews : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Message = c.String(maxLength: 512),
                        UserFullName = c.String(maxLength: 64),
                        Stamp = c.DateTime(nullable: false, storeType: "datetime2"),
                        Rating = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ProductId = c.String(maxLength: 128),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.ProductId)
                .Index(t => t.SellerId);
            
            AddColumn("dbo.Sellers", "AvarageRating", c => c.Int());
            AddColumn("dbo.Products", "AvarageRating", c => c.Int());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reviews", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Reviews", "ProductId", "dbo.Products");
            DropIndex("dbo.Reviews", new[] { "SellerId" });
            DropIndex("dbo.Reviews", new[] { "ProductId" });
            DropColumn("dbo.Products", "AvarageRating");
            DropColumn("dbo.Sellers", "AvarageRating");
            DropTable("dbo.Reviews");
        }
    }
}

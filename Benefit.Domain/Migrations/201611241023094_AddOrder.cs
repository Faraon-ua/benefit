namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderProductOptions",
                c => new
                    {
                        OrderId = c.String(nullable: false, maxLength: 128),
                        ProductOptionId = c.String(nullable: false, maxLength: 128),
                        ProductOptionName = c.String(),
                        ProductOptionPriceGrowth = c.Double(nullable: false),
                        Amount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.OrderId, t.ProductOptionId })
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Sum = c.Double(nullable: false),
                        Description = c.String(),
                        PersonalBonusesSum = c.Double(nullable: false),
                        PointsSum = c.Double(nullable: false),
                        CardNumber = c.String(),
                        ShippingName = c.String(maxLength: 32),
                        ShippingAddress = c.String(maxLength: 256),
                        ShippingCost = c.Double(nullable: false),
                        Time = c.DateTime(nullable: false, storeType: "datetime2"),
                        OrderType = c.Int(nullable: false),
                        PaymentType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        SellerId = c.String(maxLength: 128),
                        SellerName = c.String(maxLength: 128),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ApplicationUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OrderProducts",
                c => new
                    {
                        OrderId = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(nullable: false, maxLength: 128),
                        ProductName = c.String(),
                        ProductPrice = c.Double(nullable: false),
                        Amount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.OrderId, t.ProductId })
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            AddColumn("dbo.Transactions", "OrderId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Transactions", "OrderId");
            AddForeignKey("dbo.Transactions", "OrderId", "dbo.Orders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Orders", "UserId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.OrderProducts", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderProductOptions", "OrderId", "dbo.Orders");
            DropIndex("dbo.Transactions", new[] { "OrderId" });
            DropIndex("dbo.OrderProducts", new[] { "OrderId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.OrderProductOptions", new[] { "OrderId" });
            DropColumn("dbo.Transactions", "OrderId");
            DropTable("dbo.OrderProducts");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderProductOptions");
        }
    }
}

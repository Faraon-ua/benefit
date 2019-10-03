namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerTransactions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SellerTransactions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Number = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Type = c.Int(nullable: false),
                        OrderNumber = c.Int(nullable: false),
                        ProductSKU = c.Int(nullable: false),
                        ProductUrlName = c.String(),
                        Price = c.Double(nullable: false),
                        TotalPrice = c.Double(nullable: false),
                        Amount = c.Double(nullable: false),
                        Charge = c.Double(),
                        Writeoff = c.Double(),
                        Balance = c.Double(nullable: false),
                        GreyZoneBalance = c.Double(nullable: false),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SellerTransactions", "SellerId", "dbo.Sellers");
            DropIndex("dbo.SellerTransactions", new[] { "SellerId" });
            DropTable("dbo.SellerTransactions");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BusinessLevelIndexes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SellerBusinessLevelIndexes",
                c => new
                    {
                        SellerId = c.String(nullable: false, maxLength: 128),
                        BusinessLevel = c.Int(nullable: false),
                        Index = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SellerId, t.BusinessLevel })
                .ForeignKey("dbo.Sellers", t => t.SellerId, cascadeDelete: true)
                .Index(t => t.SellerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SellerBusinessLevelIndexes", "SellerId", "dbo.Sellers");
            DropIndex("dbo.SellerBusinessLevelIndexes", new[] { "SellerId" });
            DropTable("dbo.SellerBusinessLevelIndexes");
        }
    }
}

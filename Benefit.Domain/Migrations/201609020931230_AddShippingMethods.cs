namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingMethods : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ShippingMethods",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        FreeStartsFrom = c.Int(),
                        CostBeforeFree = c.Int(),
                        RegionId = c.Int(nullable: false),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Regions", t => t.RegionId, cascadeDelete: true)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.RegionId)
                .Index(t => t.SellerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ShippingMethods", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.ShippingMethods", "RegionId", "dbo.Regions");
            DropIndex("dbo.ShippingMethods", new[] { "SellerId" });
            DropIndex("dbo.ShippingMethods", new[] { "RegionId" });
            DropTable("dbo.ShippingMethods");
        }
    }
}

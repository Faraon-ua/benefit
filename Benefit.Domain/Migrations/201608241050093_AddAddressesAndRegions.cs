namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAddressesAndRegions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false, maxLength: 64),
                        Phone = c.String(nullable: false, maxLength: 16),
                        AddressLine = c.String(nullable: false, maxLength: 256),
                        ZIP = c.Int(),
                        IsDefault = c.Boolean(nullable: false),
                        RegionId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Regions", t => t.RegionId, cascadeDelete: true)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .ForeignKey("dbo.ApplicationUsers", t => t.UserId)
                .Index(t => t.RegionId)
                .Index(t => t.UserId)
                .Index(t => t.SellerId);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(),
                        PostalCode = c.String(maxLength: 6),
                        Name_ru = c.String(maxLength: 120),
                        RegionType = c.String(maxLength: 10),
                        RegionLevel = c.Short(nullable: false),
                        Name_ua = c.String(maxLength: 120),
                        Url = c.String(maxLength: 120),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Regions", t => t.ParentId)
                .Index(t => t.ParentId)
                .Index(t => t.Name_ru)
                .Index(t => t.Name_ua);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Addresses", "UserId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.Addresses", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Addresses", "RegionId", "dbo.Regions");
            DropForeignKey("dbo.Regions", "ParentId", "dbo.Regions");
            DropIndex("dbo.Regions", new[] { "Name_ua" });
            DropIndex("dbo.Regions", new[] { "Name_ru" });
            DropIndex("dbo.Regions", new[] { "ParentId" });
            DropIndex("dbo.Addresses", new[] { "SellerId" });
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropIndex("dbo.Addresses", new[] { "RegionId" });
            DropTable("dbo.Regions");
            DropTable("dbo.Addresses");
        }
    }
}

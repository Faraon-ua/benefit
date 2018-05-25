namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOldPriceToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "PrimaryRegionName", c => c.String(maxLength: 50));
            AddColumn("dbo.Sellers", "PrimaryRegionId", c => c.Int(nullable: false));
            AddColumn("dbo.Products", "OldPrice", c => c.Double());

            Sql(@"UPDATE s
                SET s.PrimaryRegionName = r.Name_ua, s.PrimaryRegionId = r.Id
                FROM Sellers s, Addresses a, Regions r
                Where a.SellerId = s.Id and a.RegionId = r.Id");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "OldPrice");
            DropColumn("dbo.Sellers", "PrimaryRegionId");
            DropColumn("dbo.Sellers", "PrimaryRegionName");
        }
    }
}

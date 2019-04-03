namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBannerSellerId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Banners", "SellerId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Banners", "SellerId");
            AddForeignKey("dbo.Banners", "SellerId", "dbo.Sellers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Banners", "SellerId", "dbo.Sellers");
            DropIndex("dbo.Banners", new[] { "SellerId" });
            DropColumn("dbo.Banners", "SellerId");
        }
    }
}

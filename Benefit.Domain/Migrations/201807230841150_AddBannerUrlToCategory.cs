namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBannerUrlToCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "BannerImageUrl", c => c.String(maxLength: 250));
            AddColumn("dbo.Categories", "BannerUrl", c => c.String(maxLength: 250));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "BannerUrl");
            DropColumn("dbo.Categories", "BannerImageUrl");
        }
    }
}

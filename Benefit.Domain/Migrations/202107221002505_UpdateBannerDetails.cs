namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateBannerDetails : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Banners", "Title", c => c.String(maxLength: 500));
            AlterColumn("dbo.Banners", "Description", c => c.String(maxLength: 1000));
            AlterColumn("dbo.StatusStamps", "Comment", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StatusStamps", "Comment", c => c.String(maxLength: 64));
            AlterColumn("dbo.Banners", "Description", c => c.String(maxLength: 100));
            AlterColumn("dbo.Banners", "Title", c => c.String(maxLength: 100));
        }
    }
}

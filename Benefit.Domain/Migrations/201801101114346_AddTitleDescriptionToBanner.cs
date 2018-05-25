namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddTitleDescriptionToBanner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Banners", "Title", c => c.String(maxLength: 100));
            AddColumn("dbo.Banners", "Description", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Banners", "Description");
            DropColumn("dbo.Banners", "Title");
        }
    }
}

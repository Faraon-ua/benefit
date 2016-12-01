namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddProductSearchTags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "SearchTags", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "SearchTags");
        }
    }
}

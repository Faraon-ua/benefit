namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddChildCategoriesAsFilters : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "ChildAsFilters", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "ChildAsFilters");
        }
    }
}

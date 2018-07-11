namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryTag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "Tag", c => c.String(maxLength: 250));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "Tag");
        }
    }
}

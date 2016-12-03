namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Products", "SearchTags");
            CreateIndex("dbo.Categories", "Name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Categories", new[] { "Name" });
            DropIndex("dbo.Products", new[] { "SearchTags" });
        }
    }
}

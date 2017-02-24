namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeoProperties : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Products", new[] { "SearchTags" });
            AddColumn("dbo.Sellers", "ShortSescription", c => c.String(maxLength: 160));
            AddColumn("dbo.Sellers", "KeyWords", c => c.String(maxLength: 160));
            AddColumn("dbo.Products", "ShortDescription", c => c.String(maxLength: 160));
            AddColumn("dbo.InfoPages", "Keywords", c => c.String(maxLength: 160));
            AlterColumn("dbo.Products", "SearchTags", c => c.String(maxLength: 160));
            CreateIndex("dbo.Products", "SearchTags");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Products", new[] { "SearchTags" });
            AlterColumn("dbo.Products", "SearchTags", c => c.String(maxLength: 256));
            DropColumn("dbo.InfoPages", "Keywords");
            DropColumn("dbo.Products", "ShortDescription");
            DropColumn("dbo.Sellers", "KeyWords");
            DropColumn("dbo.Sellers", "ShortSescription");
            CreateIndex("dbo.Products", "SearchTags");
        }
    }
}

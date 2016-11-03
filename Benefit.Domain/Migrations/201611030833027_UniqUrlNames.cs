namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqUrlNames : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Sellers", "UrlName", unique: true);
            CreateIndex("dbo.Products", "UrlName", unique: true);
            CreateIndex("dbo.Categories", "UrlName", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Categories", new[] { "UrlName" });
            DropIndex("dbo.Products", new[] { "UrlName" });
            DropIndex("dbo.Sellers", new[] { "UrlName" });
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SkuToUnique : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Products", new[] { "SKU" });
            CreateIndex("dbo.Products", "SKU", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Products", new[] { "SKU" });
            CreateIndex("dbo.Products", "SKU");
        }
    }
}

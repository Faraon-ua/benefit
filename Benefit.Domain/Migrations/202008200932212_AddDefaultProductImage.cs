namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDefaultProductImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "DefaultImageId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Products", "DefaultImageId");
            AddForeignKey("dbo.Products", "DefaultImageId", "dbo.Images", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "DefaultImageId", "dbo.Images");
            DropIndex("dbo.Products", new[] { "DefaultImageId" });
            DropColumn("dbo.Products", "DefaultImageId");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategories : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 64),
                        UrlName = c.String(nullable: false, maxLength: 128),
                        NavigationType = c.String(nullable: false, maxLength: 32),
                        Description = c.String(nullable: false, maxLength: 256),
                        ImageUrl = c.String(),
                        Order = c.Int(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                        LastModified = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastModifiedBy = c.String(),
                        ParentCategoryId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.ParentCategoryId)
                .Index(t => t.ParentCategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Categories", "ParentCategoryId", "dbo.Categories");
            DropIndex("dbo.Categories", new[] { "ParentCategoryId" });
            DropTable("dbo.Categories");
        }
    }
}

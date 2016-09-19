namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductParameters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductParameters",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 64),
                        UrlName = c.String(nullable: false, maxLength: 64),
                        Order = c.Int(),
                        DisplayInFilters = c.Boolean(nullable: false),
                        MeasureUnit = c.String(maxLength: 16),
                        Type = c.String(maxLength: 32),
                        IsVerified = c.Boolean(nullable: false),
                        AddedById = c.String(maxLength: 128),
                        ParentProductParameterId = c.String(maxLength: 128),
                        CategoryId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ApplicationUsers", t => t.AddedById)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.ProductParameters", t => t.ParentProductParameterId)
                .Index(t => t.AddedById)
                .Index(t => t.ParentProductParameterId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductParameterProducts",
                c => new
                    {
                        ProductParameterId = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(nullable: false, maxLength: 128),
                        Amount = c.Int(),
                        StartValue = c.String(),
                        EndValue = c.String(),
                    })
                .PrimaryKey(t => new { t.ProductParameterId, t.ProductId })
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.ProductParameters", t => t.ProductParameterId, cascadeDelete: true)
                .Index(t => t.ProductParameterId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.ProductParameterValues",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ParameterValue = c.String(maxLength: 64),
                        ParameterValueUrl = c.String(maxLength: 64),
                        IsVerified = c.Boolean(nullable: false),
                        AddedById = c.String(maxLength: 128),
                        ProductParameterId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ApplicationUsers", t => t.AddedById)
                .ForeignKey("dbo.ProductParameters", t => t.ProductParameterId)
                .Index(t => t.AddedById)
                .Index(t => t.ProductParameterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductParameterValues", "ProductParameterId", "dbo.ProductParameters");
            DropForeignKey("dbo.ProductParameterValues", "AddedById", "dbo.ApplicationUsers");
            DropForeignKey("dbo.ProductParameterProducts", "ProductParameterId", "dbo.ProductParameters");
            DropForeignKey("dbo.ProductParameterProducts", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters");
            DropForeignKey("dbo.ProductParameters", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.ProductParameters", "AddedById", "dbo.ApplicationUsers");
            DropIndex("dbo.ProductParameterValues", new[] { "ProductParameterId" });
            DropIndex("dbo.ProductParameterValues", new[] { "AddedById" });
            DropIndex("dbo.ProductParameterProducts", new[] { "ProductId" });
            DropIndex("dbo.ProductParameterProducts", new[] { "ProductParameterId" });
            DropIndex("dbo.ProductParameters", new[] { "CategoryId" });
            DropIndex("dbo.ProductParameters", new[] { "ParentProductParameterId" });
            DropIndex("dbo.ProductParameters", new[] { "AddedById" });
            DropTable("dbo.ProductParameterValues");
            DropTable("dbo.ProductParameterProducts");
            DropTable("dbo.ProductParameters");
        }
    }
}

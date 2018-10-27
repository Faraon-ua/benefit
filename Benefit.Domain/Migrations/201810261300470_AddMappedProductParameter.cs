namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMappedProductParameter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MappedProductParameters",
                c => new
                    {
                        SellerId = c.String(nullable: false, maxLength: 128),
                        ProductParameterId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => new { t.SellerId, t.ProductParameterId })
                .ForeignKey("dbo.ProductParameters", t => t.ProductParameterId, cascadeDelete: true)
                .ForeignKey("dbo.Sellers", t => t.SellerId, cascadeDelete: true)
                .Index(t => t.SellerId)
                .Index(t => t.ProductParameterId);
            
            AddColumn("dbo.ProductParameters", "ParentProductParameterId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ProductParameters", "ParentProductParameterId");
            AddForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MappedProductParameters", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.MappedProductParameters", "ProductParameterId", "dbo.ProductParameters");
            DropForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters");
            DropIndex("dbo.MappedProductParameters", new[] { "ProductParameterId" });
            DropIndex("dbo.MappedProductParameters", new[] { "SellerId" });
            DropIndex("dbo.ProductParameters", new[] { "ParentProductParameterId" });
            DropColumn("dbo.ProductParameters", "ParentProductParameterId");
            DropTable("dbo.MappedProductParameters");
        }
    }
}

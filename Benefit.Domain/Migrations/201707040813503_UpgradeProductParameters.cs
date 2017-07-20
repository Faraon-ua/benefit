namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpgradeProductParameters : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProductParameters", "AddedById", "dbo.ApplicationUsers");
            DropForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters");
            DropForeignKey("dbo.ProductParameterValues", "AddedById", "dbo.ApplicationUsers");
            DropIndex("dbo.ProductParameters", new[] { "AddedById" });
            DropIndex("dbo.ProductParameters", new[] { "ParentProductParameterId" });
            DropIndex("dbo.ProductParameterValues", new[] { "AddedById" });
            DropPrimaryKey("dbo.ProductParameterProducts");
            AddColumn("dbo.Products", "Vendor", c => c.String(maxLength: 128));
            AddColumn("dbo.ProductParameters", "AddedBy", c => c.String(maxLength: 128));
            AddColumn("dbo.ProductParameterProducts", "StartText", c => c.String(maxLength: 64));
            AddColumn("dbo.ProductParameterProducts", "EndText", c => c.String(maxLength: 64));
            AddColumn("dbo.ProductParameterValues", "AddedBy", c => c.String(maxLength: 128));
            AlterColumn("dbo.ProductParameterProducts", "StartValue", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.ProductParameterProducts", "EndValue", c => c.String(maxLength: 64));
            AddPrimaryKey("dbo.ProductParameterProducts", new[] { "ProductParameterId", "ProductId", "StartValue" });
            CreateIndex("dbo.ProductParameters", "UrlName");
            CreateIndex("dbo.ProductParameterProducts", "EndValue");
            DropColumn("dbo.ProductParameters", "AddedById");
            DropColumn("dbo.ProductParameters", "ParentProductParameterId");
            DropColumn("dbo.ProductParameterValues", "AddedById");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductParameterValues", "AddedById", c => c.String(maxLength: 128));
            AddColumn("dbo.ProductParameters", "ParentProductParameterId", c => c.String(maxLength: 128));
            AddColumn("dbo.ProductParameters", "AddedById", c => c.String(maxLength: 128));
            DropIndex("dbo.ProductParameterProducts", new[] { "EndValue" });
            DropIndex("dbo.ProductParameters", new[] { "UrlName" });
            DropPrimaryKey("dbo.ProductParameterProducts");
            AlterColumn("dbo.ProductParameterProducts", "EndValue", c => c.String());
            AlterColumn("dbo.ProductParameterProducts", "StartValue", c => c.String());
            DropColumn("dbo.ProductParameterValues", "AddedBy");
            DropColumn("dbo.ProductParameterProducts", "EndText");
            DropColumn("dbo.ProductParameterProducts", "StartText");
            DropColumn("dbo.ProductParameters", "AddedBy");
            DropColumn("dbo.Products", "Vendor");
            AddPrimaryKey("dbo.ProductParameterProducts", new[] { "ProductParameterId", "ProductId" });
            CreateIndex("dbo.ProductParameterValues", "AddedById");
            CreateIndex("dbo.ProductParameters", "ParentProductParameterId");
            CreateIndex("dbo.ProductParameters", "AddedById");
            AddForeignKey("dbo.ProductParameterValues", "AddedById", "dbo.ApplicationUsers", "Id");
            AddForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters", "Id");
            AddForeignKey("dbo.ProductParameters", "AddedById", "dbo.ApplicationUsers", "Id");
        }
    }
}

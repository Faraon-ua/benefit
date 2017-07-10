namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpgradeProductParameters : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters");
            DropIndex("dbo.ProductParameters", new[] { "ParentProductParameterId" });
            DropPrimaryKey("dbo.ProductParameterProducts");
            AddColumn("dbo.ProductParameterProducts", "StartText", c => c.String(maxLength: 32));
            AddColumn("dbo.ProductParameterProducts", "EndText", c => c.String(maxLength: 32));
            AlterColumn("dbo.ProductParameterProducts", "StartValue", c => c.String(nullable: false, maxLength: 16));
            AlterColumn("dbo.ProductParameterProducts", "EndValue", c => c.String(maxLength: 16));
            AddPrimaryKey("dbo.ProductParameterProducts", new[] { "ProductParameterId", "ProductId", "StartValue" });
            CreateIndex("dbo.ProductParameters", "UrlName");
            CreateIndex("dbo.ProductParameterProducts", "EndValue");
            DropColumn("dbo.ProductParameters", "ParentProductParameterId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductParameters", "ParentProductParameterId", c => c.String(maxLength: 128));
            DropIndex("dbo.ProductParameterProducts", new[] { "EndValue" });
            DropIndex("dbo.ProductParameters", new[] { "UrlName" });
            DropPrimaryKey("dbo.ProductParameterProducts");
            AlterColumn("dbo.ProductParameterProducts", "EndValue", c => c.String());
            AlterColumn("dbo.ProductParameterProducts", "StartValue", c => c.String());
            DropColumn("dbo.ProductParameterProducts", "EndText");
            DropColumn("dbo.ProductParameterProducts", "StartText");
            AddPrimaryKey("dbo.ProductParameterProducts", new[] { "ProductParameterId", "ProductId" });
            CreateIndex("dbo.ProductParameters", "ParentProductParameterId");
            AddForeignKey("dbo.ProductParameters", "ParentProductParameterId", "dbo.ProductParameters", "Id");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendProductParameterValueStartText : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.ProductParameterProducts");
            AlterColumn("dbo.ProductParameterProducts", "StartValue", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ProductParameterProducts", "StartText", c => c.String(maxLength: 150));
            AddPrimaryKey("dbo.ProductParameterProducts", new[] { "ProductParameterId", "ProductId", "StartValue" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.ProductParameterProducts");
            AlterColumn("dbo.ProductParameterProducts", "StartText", c => c.String(maxLength: 64));
            AlterColumn("dbo.ProductParameterProducts", "StartValue", c => c.String(nullable: false, maxLength: 64));
            AddPrimaryKey("dbo.ProductParameterProducts", new[] { "ProductParameterId", "ProductId", "StartValue" });
        }
    }
}

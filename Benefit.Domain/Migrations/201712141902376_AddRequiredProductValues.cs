namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequiredProductValues : DbMigration
    {
        public override void Up()
        {
            Sql("Update ProductParameterValues set ParameterValueUrl = ParameterValue where ParameterValueUrl is null");
            AlterColumn("dbo.ProductParameterValues", "ParameterValue", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.ProductParameterValues", "ParameterValueUrl", c => c.String(nullable: false, maxLength: 64));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProductParameterValues", "ParameterValueUrl", c => c.String(maxLength: 64));
            AlterColumn("dbo.ProductParameterValues", "ParameterValue", c => c.String(maxLength: 64));
        }
    }
}

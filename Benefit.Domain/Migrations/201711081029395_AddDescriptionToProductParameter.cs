namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDescriptionToProductParameter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductParameters", "Description", c => c.String(maxLength: 160));
            AddColumn("dbo.ProductParameterValues", "Order", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductParameterValues", "Order");
            DropColumn("dbo.ProductParameters", "Description");
        }
    }
}

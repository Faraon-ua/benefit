namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderProductOptionAddPrimaryKey : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.OrderProductOptions");
            AlterColumn("dbo.OrderProductOptions", "ProductId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.OrderProductOptions", new[] { "OrderId", "ProductOptionId", "ProductId" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.OrderProductOptions");
            AlterColumn("dbo.OrderProductOptions", "ProductId", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.OrderProductOptions", new[] { "OrderId", "ProductOptionId" });
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeOrderProductKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrderProductOptions", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderProducts", "OrderId", "dbo.Orders");
            DropIndex("dbo.OrderProductOptions", new[] { "OrderId" });
            DropIndex("dbo.OrderProducts", new[] { "OrderId" });
            DropPrimaryKey("dbo.OrderProductOptions");
            DropPrimaryKey("dbo.OrderProducts");
            AddColumn("dbo.OrderProductOptions", "OrderProductId", c => c.String(nullable: false, maxLength: 128, defaultValueSql: "newid()"));
            AddColumn("dbo.OrderProducts", "Id", c => c.String(nullable: false, maxLength: 128, defaultValueSql: "newid()"));
            AlterColumn("dbo.OrderProducts", "OrderId", c => c.String(maxLength: 128));
            AlterColumn("dbo.OrderProducts", "ProductId", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.OrderProductOptions", new[] { "ProductOptionId", "OrderProductId" });
            AddPrimaryKey("dbo.OrderProducts", "Id");
            CreateIndex("dbo.OrderProducts", "OrderId");
            CreateIndex("dbo.OrderProductOptions", "OrderProductId");
            Sql("delete from OrderProductOptions");
            AddForeignKey("dbo.OrderProductOptions", "OrderProductId", "dbo.OrderProducts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.OrderProducts", "OrderId", "dbo.Orders", "Id");
            DropColumn("dbo.OrderProductOptions", "OrderId");
            DropColumn("dbo.OrderProductOptions", "ProductId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OrderProductOptions", "ProductId", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.OrderProductOptions", "OrderId", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.OrderProducts", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderProductOptions", "OrderProductId", "dbo.OrderProducts");
            DropIndex("dbo.OrderProductOptions", new[] { "OrderProductId" });
            DropIndex("dbo.OrderProducts", new[] { "OrderId" });
            DropPrimaryKey("dbo.OrderProducts");
            DropPrimaryKey("dbo.OrderProductOptions");
            AlterColumn("dbo.OrderProducts", "ProductId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.OrderProducts", "OrderId", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.OrderProducts", "Id");
            DropColumn("dbo.OrderProductOptions", "OrderProductId");
            AddPrimaryKey("dbo.OrderProducts", new[] { "OrderId", "ProductId" });
            AddPrimaryKey("dbo.OrderProductOptions", new[] { "OrderId", "ProductOptionId", "ProductId" });
            CreateIndex("dbo.OrderProducts", "OrderId");
            CreateIndex("dbo.OrderProductOptions", "OrderId");
            AddForeignKey("dbo.OrderProducts", "OrderId", "dbo.Orders", "Id", cascadeDelete: true);
            AddForeignKey("dbo.OrderProductOptions", "OrderId", "dbo.Orders", "Id", cascadeDelete: true);
        }
    }
}

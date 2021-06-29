namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ModifyStatusStamp : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrderStatusStamps", "OrderId", "dbo.Orders");
            RenameTable(name: "dbo.OrderStatusStamps", newName: "StatusStamps");
            DropIndex("dbo.StatusStamps", new[] { "OrderId" });
            AlterColumn("dbo.StatusStamps", "OrderId", c => c.String(maxLength: 128));
            CreateIndex("dbo.StatusStamps", "OrderId");
            AddForeignKey("dbo.StatusStamps", "OrderId", "dbo.Orders", "Id");

            AddColumn("dbo.StatusStamps", "ProductId", c => c.String(maxLength: 128));
            AddForeignKey("dbo.StatusStamps", "ProductId", "dbo.Products", "Id");
            CreateIndex("dbo.StatusStamps", "ProductId");

            RenameColumn("dbo.StatusStamps", "OrderStatus", "Status");
            DropColumn("dbo.SellerCategories", "IsDefault");
        }

        public override void Down()
        {
            RenameColumn("dbo.StatusStamps", "Status", "OrderStatus");
            AddColumn("dbo.SellerCategories", "IsDefault", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.StatusStamps", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.StatusStamps", "ProductId", "dbo.Products");
            DropIndex("dbo.StatusStamps", new[] { "ProductId" });
            DropIndex("dbo.StatusStamps", new[] { "OrderId" });
            AlterColumn("dbo.StatusStamps", "OrderId", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.StatusStamps", "ProductId");
            CreateIndex("dbo.StatusStamps", "OrderId");
            AddForeignKey("dbo.OrderStatusStamps", "OrderId", "dbo.Orders", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.StatusStamps", newName: "OrderStatusStamps");
        }
    }
}

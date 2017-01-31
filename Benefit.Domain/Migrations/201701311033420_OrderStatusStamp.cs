namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderStatusStamp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderStatusStamps",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OrderId = c.String(nullable: false, maxLength: 128),
                        OrderStatus = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false, storeType: "datetime2"),
                        UpdatedBy = c.String(maxLength: 32),
                        Comment = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            DropColumn("dbo.Orders", "StatusComment");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "StatusComment", c => c.String(maxLength: 64));
            DropForeignKey("dbo.OrderStatusStamps", "OrderId", "dbo.Orders");
            DropIndex("dbo.OrderStatusStamps", new[] { "OrderId" });
            DropTable("dbo.OrderStatusStamps");
        }
    }
}

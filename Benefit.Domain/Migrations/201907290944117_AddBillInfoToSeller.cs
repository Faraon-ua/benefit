namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBillInfoToSeller : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentBills",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Number = c.String(nullable: false),
                        InnerNumber = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Type = c.Int(nullable: false),
                        Sum = c.Double(nullable: false),
                        Status = c.Int(nullable: false),
                        Description = c.String(maxLength: 500),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId);
            
            AddColumn("dbo.Sellers", "CurrentBill", c => c.Double(nullable: false));
            AddColumn("dbo.Sellers", "GreyZone", c => c.Double(nullable: false));
            AddColumn("dbo.Orders", "ShippingTrackingNumber", c => c.String(maxLength: 64));
            AddColumn("dbo.Transactions", "SellerId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Transactions", "SellerId");
            AddForeignKey("dbo.Transactions", "SellerId", "dbo.Sellers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PaymentBills", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Transactions", "SellerId", "dbo.Sellers");
            DropIndex("dbo.PaymentBills", new[] { "SellerId" });
            DropIndex("dbo.Transactions", new[] { "SellerId" });
            DropColumn("dbo.Transactions", "SellerId");
            DropColumn("dbo.Orders", "ShippingTrackingNumber");
            DropColumn("dbo.Sellers", "GreyZone");
            DropColumn("dbo.Sellers", "CurrentBill");
            DropTable("dbo.PaymentBills");
        }
    }
}

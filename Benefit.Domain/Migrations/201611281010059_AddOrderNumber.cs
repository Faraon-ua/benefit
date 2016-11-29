namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderNumber : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Personnels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 32),
                        Phone = c.String(maxLength: 16),
                        CardNumber = c.String(maxLength: 10),
                        NFCCardNumber = c.String(maxLength: 10),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId);
            
            AddColumn("dbo.Sellers", "TerminalLogin", c => c.String(maxLength: 32));
            AddColumn("dbo.Sellers", "TerminalPassword", c => c.String(maxLength: 16));
            AddColumn("dbo.OrderProductOptions", "ProductId", c => c.String(maxLength: 128));
            AddColumn("dbo.Orders", "OrderNumber", c => c.Int(nullable: false));
            AddColumn("dbo.Orders", "StatusComment", c => c.String(maxLength: 64));
            AddColumn("dbo.Orders", "PersonnelId", c => c.String(maxLength: 128));
            AddColumn("dbo.Orders", "PersonnelName", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Personnels", "SellerId", "dbo.Sellers");
            DropIndex("dbo.Personnels", new[] { "SellerId" });
            DropColumn("dbo.Orders", "PersonnelName");
            DropColumn("dbo.Orders", "PersonnelId");
            DropColumn("dbo.Orders", "StatusComment");
            DropColumn("dbo.Orders", "OrderNumber");
            DropColumn("dbo.OrderProductOptions", "ProductId");
            DropColumn("dbo.Sellers", "TerminalPassword");
            DropColumn("dbo.Sellers", "TerminalLogin");
            DropTable("dbo.Personnels");
        }
    }
}

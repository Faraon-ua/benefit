namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCurrencies : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 10),
                        Provider = c.String(nullable: false, maxLength: 16),
                        Rate = c.Double(nullable: false),
                        SellerId = c.String(maxLength: 128)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId);
            
            AddColumn("dbo.Products", "CurrencyId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Products", "CurrencyId");
            AddForeignKey("dbo.Products", "CurrencyId", "dbo.Currencies", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Currencies", "SellerId", "dbo.Sellers");
            DropForeignKey("dbo.Products", "CurrencyId", "dbo.Currencies");
            DropIndex("dbo.Currencies", new[] { "SellerId" });
            DropIndex("dbo.Products", new[] { "CurrencyId" });
            DropColumn("dbo.Products", "CurrencyId");
            DropTable("dbo.Currencies");
        }
    }
}

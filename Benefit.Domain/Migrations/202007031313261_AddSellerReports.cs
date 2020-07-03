namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerReports : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SellerReports",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SellerId = c.String(maxLength: 128),
                        Month = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        FileUrl = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SellerReports", "SellerId", "dbo.Sellers");
            DropIndex("dbo.SellerReports", new[] { "SellerId" });
            DropTable("dbo.SellerReports");
        }
    }
}

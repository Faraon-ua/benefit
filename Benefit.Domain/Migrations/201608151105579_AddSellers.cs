namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sellers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        Description = c.String(nullable: false),
                        UrlName = c.String(nullable: false, maxLength: 128),
                        CatalogButtonName = c.String(nullable: false, maxLength: 16),
                        IsActive = c.Boolean(nullable: false),
                        IsBenefitCardActive = c.Boolean(nullable: false),
                        HasEcommerce = c.Boolean(nullable: false),
                        TotalDiscount = c.Int(nullable: false),
                        UserDiscount = c.Double(nullable: false),
                        RegisteredOn = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastModified = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastModifiedBy = c.String(),
                        OwnerId = c.String(nullable: false, maxLength: 128),
                        BenefitCardReferalId = c.String(maxLength: 128),
                        WebSiteReferalId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ApplicationUsers", t => t.BenefitCardReferalId)
                .ForeignKey("dbo.ApplicationUsers", t => t.OwnerId)
                .ForeignKey("dbo.ApplicationUsers", t => t.WebSiteReferalId)
                .Index(t => t.Name)
                .Index(t => t.OwnerId)
                .Index(t => t.BenefitCardReferalId)
                .Index(t => t.WebSiteReferalId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Sellers", "WebSiteReferalId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.Sellers", "OwnerId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.Sellers", "BenefitCardReferalId", "dbo.ApplicationUsers");
            DropIndex("dbo.Sellers", new[] { "WebSiteReferalId" });
            DropIndex("dbo.Sellers", new[] { "BenefitCardReferalId" });
            DropIndex("dbo.Sellers", new[] { "OwnerId" });
            DropIndex("dbo.Sellers", new[] { "Name" });
            DropTable("dbo.Sellers");
        }
    }
}

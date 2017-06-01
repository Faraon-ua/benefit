namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UpgradePromotion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PromotionAccomplishments",
                c => new
                    {
                        PromotionId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        AccomplishmentsNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PromotionId, t.UserId })
                .ForeignKey("dbo.Promotions", t => t.PromotionId, cascadeDelete: true)
                .ForeignKey("dbo.ApplicationUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.PromotionId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Promotions", "IsBonusDiscount", c => c.Boolean(nullable: false));
            AddColumn("dbo.Promotions", "ShouldBeVisibleInStructure", c => c.Boolean(nullable: false));
            AddColumn("dbo.Promotions", "Level", c => c.Int(nullable: false));
            RenameColumn("dbo.Promotions", "InstantDiscountFrom", "DiscountFrom");
            RenameColumn("dbo.Promotions", "InstantDiscountValue", "DiscountValue");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Promotions", "DiscountFrom", "InstantDiscountFrom");
            RenameColumn("dbo.Promotions", "DiscountValue", "InstantDiscountValue");
            DropForeignKey("dbo.PromotionAccomplishments", "UserId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.PromotionAccomplishments", "PromotionId", "dbo.Promotions");
            DropIndex("dbo.PromotionAccomplishments", new[] { "UserId" });
            DropIndex("dbo.PromotionAccomplishments", new[] { "PromotionId" });
            DropColumn("dbo.Promotions", "Level");
            DropColumn("dbo.Promotions", "ShouldBeVisibleInStructure");
            DropColumn("dbo.Promotions", "IsBonusDiscount");
            DropTable("dbo.PromotionAccomplishments");
        }
    }
}

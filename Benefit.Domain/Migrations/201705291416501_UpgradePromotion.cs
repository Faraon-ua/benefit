namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class UpgradePromotion : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Promotions", "InstantDiscountFrom", "DiscountFrom");
            RenameColumn("dbo.Promotions", "InstantDiscountValue", "DiscountValue");
            AddColumn("dbo.Promotions", "IsInstantDiscount", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Promotions", "Level", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            RenameColumn("dbo.Promotions", "DiscountFrom", "InstantDiscountFrom");
            RenameColumn("dbo.Promotions", "DiscountValue", "InstantDiscountValue");
            DropColumn("dbo.Promotions", "Level");
            DropColumn("dbo.Promotions", "IsInstantDiscount");
        }
    }
}

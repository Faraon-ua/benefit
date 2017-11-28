namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMentorPromotionOption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Promotions", "IsCurrentAccountBonusPromotion", c => c.Boolean(nullable: false));
            AddColumn("dbo.Promotions", "IsMentorPromotion", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotions", "IsMentorPromotion");
            DropColumn("dbo.Promotions", "IsCurrentAccountBonusPromotion");
        }
    }
}

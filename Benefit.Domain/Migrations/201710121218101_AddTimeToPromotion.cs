namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTimeToPromotion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Promotions", "StartTime", c => c.Short());
            AddColumn("dbo.Promotions", "EndTime", c => c.Short());
            AddColumn("dbo.Promotions", "IsValuePercent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotions", "IsValuePercent");
            DropColumn("dbo.Promotions", "EndTime");
            DropColumn("dbo.Promotions", "StartTime");
        }
    }
}

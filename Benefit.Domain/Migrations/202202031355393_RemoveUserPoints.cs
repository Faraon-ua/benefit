namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserPoints : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "IsProcessed", c => c.Boolean(nullable: false));
            DropColumn("dbo.Sellers", "PointsAccount");
            DropColumn("dbo.Sellers", "HangingPointsAccount");
            DropColumn("dbo.ApplicationUsers", "TotalBonusAccount");
            DropColumn("dbo.ApplicationUsers", "CurrentBonusAccount");
            DropColumn("dbo.ApplicationUsers", "PointsAccount");
            DropColumn("dbo.ApplicationUsers", "HangingPointsAccount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ApplicationUsers", "HangingPointsAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "PointsAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "CurrentBonusAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "TotalBonusAccount", c => c.Double(nullable: false));
            AddColumn("dbo.Sellers", "HangingPointsAccount", c => c.Double(nullable: false));
            AddColumn("dbo.Sellers", "PointsAccount", c => c.Double(nullable: false));
            DropColumn("dbo.Transactions", "IsProcessed");
        }
    }
}

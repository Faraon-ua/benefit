namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPointsToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "PointsAccount", c => c.Double(nullable: false));
            AddColumn("dbo.Sellers", "HangingPointsAccount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "HangingPointsAccount");
            DropColumn("dbo.Sellers", "PointsAccount");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrationIsRecommendedProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "GenerateRecommendedProducts", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "IsRecommended", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "IsRecommended");
            DropColumn("dbo.Sellers", "GenerateRecommendedProducts");
        }
    }
}

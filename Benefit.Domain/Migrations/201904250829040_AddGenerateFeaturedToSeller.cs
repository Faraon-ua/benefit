namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGenerateFeaturedToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "GenerateFeaturedProducts", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "GenerateFeaturedProducts");
        }
    }
}

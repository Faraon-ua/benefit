namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsFeaturedToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "IsFeatured", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "IsFeatured");
        }
    }
}

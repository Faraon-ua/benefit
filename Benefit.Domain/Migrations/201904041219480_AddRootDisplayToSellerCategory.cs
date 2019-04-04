namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRootDisplayToSellerCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SellerCategories", "RootDisplay", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SellerCategories", "RootDisplay");
        }
    }
}

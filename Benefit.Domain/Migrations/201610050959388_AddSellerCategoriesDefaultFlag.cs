namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerCategoriesDefaultFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SellerCategories", "IsDefault", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SellerCategories", "IsDefault");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendSellerCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SellerCategories", "CustomName", c => c.String(maxLength: 64));
            AddColumn("dbo.SellerCategories", "CustomImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SellerCategories", "CustomImageUrl");
            DropColumn("dbo.SellerCategories", "CustomName");
        }
    }
}

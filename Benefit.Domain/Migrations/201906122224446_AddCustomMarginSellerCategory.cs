namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomMarginSellerCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SellerCategories", "CustomMargin", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SellerCategories", "CustomMargin");
        }
    }
}

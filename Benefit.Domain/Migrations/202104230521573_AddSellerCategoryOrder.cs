namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerCategoryOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SellerCategories", "Order", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SellerCategories", "Order");
        }
    }
}

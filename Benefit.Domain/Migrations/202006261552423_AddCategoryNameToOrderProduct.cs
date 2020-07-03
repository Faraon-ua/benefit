namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryNameToOrderProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderProducts", "CategoryName", c => c.String(maxLength: 64));
            AddColumn("dbo.SellerTransactions", "FeePercent", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SellerTransactions", "FeePercent");
            DropColumn("dbo.OrderProducts", "CategoryName");
        }
    }
}

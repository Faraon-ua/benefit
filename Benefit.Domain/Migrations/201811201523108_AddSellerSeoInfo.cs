namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerSeoInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "SeoSuffix", c => c.String(maxLength: 250));
            Sql("Update Orders set ShippingAddress = 'уточнити у клієнта' Where ShippingAddress is NULL");
            AlterColumn("dbo.Orders", "ShippingAddress", c => c.String(nullable: false, maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "ShippingAddress", c => c.String(maxLength: 256));
            DropColumn("dbo.Sellers", "SeoSuffix");
        }
    }
}

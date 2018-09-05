namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerEcommerceTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "EcommerceTemplate", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "EcommerceTemplate");
        }
    }
}

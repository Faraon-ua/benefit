namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPromoPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PromoPrice", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "PromoPrice");
        }
    }
}

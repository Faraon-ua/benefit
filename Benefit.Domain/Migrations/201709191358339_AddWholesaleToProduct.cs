namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWholesaleToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "WholesalePrice", c => c.Double());
            AddColumn("dbo.Products", "WholesaleFrom", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "WholesaleFrom");
            DropColumn("dbo.Products", "WholesalePrice");
        }
    }
}

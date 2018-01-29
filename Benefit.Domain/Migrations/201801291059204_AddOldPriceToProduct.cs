namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOldPriceToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "OldPrice", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "OldPrice");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSafePurchaseToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "SafePurchase", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "SafePurchase");
        }
    }
}

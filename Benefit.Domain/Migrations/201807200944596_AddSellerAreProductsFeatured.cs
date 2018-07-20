namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerAreProductsFeatured : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "AreProductsFeatured", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "AreProductsFeatured");
        }
    }
}

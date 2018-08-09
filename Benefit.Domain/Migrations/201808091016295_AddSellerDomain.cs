namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerDomain : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Domain", c => c.String(maxLength: 60));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "Domain");
        }
    }
}

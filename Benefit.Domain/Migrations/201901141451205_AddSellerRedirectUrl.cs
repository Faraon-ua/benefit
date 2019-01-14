namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerRedirectUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "RedirectUrl", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "RedirectUrl");
        }
    }
}

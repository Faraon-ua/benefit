namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGoogleSiteVerificationToken : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "GoogleSiteVerificationToken", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "GoogleSiteVerificationToken");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerLegalInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "LegalName", c => c.String(nullable: false, maxLength: 128, defaultValue:"N/A"));
            AddColumn("dbo.Sellers", "LegalDescription", c => c.String(nullable: false, maxLength: 500, defaultValue: "N/A"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "LegalDescription");
            DropColumn("dbo.Sellers", "LegalName");
        }
    }
}

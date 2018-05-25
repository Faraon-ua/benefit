namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalIdToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ExternalId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "ExternalId");
        }
    }
}

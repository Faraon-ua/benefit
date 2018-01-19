namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CategoryExternalIds : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Categories", "ExternalId", "ExternalIds");
            AlterColumn("dbo.Categories", "ExternalIds", c => c.String());
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Categories", "ExternalIds", "ExternalId");
        }
    }
}
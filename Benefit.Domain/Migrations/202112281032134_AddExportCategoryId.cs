namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExportCategoryId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExportCategories", "ExternalId", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportCategories", "ExternalId");
        }
    }
}

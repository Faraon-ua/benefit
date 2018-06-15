namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHasNewContentToImport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExportImports", "HasNewContent", c => c.Boolean(nullable: false));
            DropColumn("dbo.Categories", "NavigationType");
            DropColumn("dbo.ExportImports", "RemoveProducts");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExportImports", "RemoveProducts", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "NavigationType", c => c.String(nullable: false, maxLength: 32));
            AddColumn("dbo.Categories", "NavigationType", c => c.String(nullable: false, maxLength: 32));
            DropColumn("dbo.ExportImports", "HasNewContent");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMetaDescriptionCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "MetaDescription", c => c.String(maxLength: 256));
            AlterColumn("dbo.Categories", "Description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Categories", "Description", c => c.String(nullable: false, maxLength: 256));
            DropColumn("dbo.Categories", "MetaDescription");
        }
    }
}

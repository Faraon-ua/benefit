namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTitleDescriptionLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "Title", c => c.String(maxLength: 100));
            AlterColumn("dbo.Categories", "Title", c => c.String(maxLength: 100));
            AlterColumn("dbo.InfoPages", "Title", c => c.String(maxLength: 100));
            AlterColumn("dbo.Products", "ShortDescription", c => c.String(maxLength: 210));
        }

        public override void Down()
        {
            AlterColumn("dbo.InfoPages", "Title", c => c.String(maxLength: 70));
            AlterColumn("dbo.Categories", "Title", c => c.String(maxLength: 70));
            AlterColumn("dbo.Products", "Title", c => c.String(maxLength: 70));
            AlterColumn("dbo.Products", "ShortDescription", c => c.String(maxLength: 160));
        }
    }
}

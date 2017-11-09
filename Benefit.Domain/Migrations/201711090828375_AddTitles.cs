namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTitles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Title", c => c.String(maxLength: 70));
            AddColumn("dbo.Categories", "Title", c => c.String(maxLength: 70));
            AddColumn("dbo.InfoPages", "Title", c => c.String(maxLength: 70));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InfoPages", "Title");
            DropColumn("dbo.Categories", "Title");
            DropColumn("dbo.Products", "Title");
        }
    }
}

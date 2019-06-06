namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddModerationToProduct : DbMigration
    {
        public override void Up()
        {
            
            AddColumn("dbo.Products", "Comment", c => c.String(maxLength: 250));
            AddColumn("dbo.Products", "ModerationStatus", c => c.Int(nullable: false));
            DropColumn("dbo.Products", "LastModifiedBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "LastModifiedBy", c => c.String(maxLength: 64));
            DropColumn("dbo.Products", "ModerationStatus");
            DropColumn("dbo.Products", "Comment");
        }
    }
}

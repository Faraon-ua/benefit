namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalCategoryId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "ExternalId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "ExternalId");
        }
    }
}

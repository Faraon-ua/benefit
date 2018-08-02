namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsImportedToImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Images", "IsImported", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Images", "IsImported");
        }
    }
}

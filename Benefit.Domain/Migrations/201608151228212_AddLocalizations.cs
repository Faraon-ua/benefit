namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocalizations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Localizations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ResourceType = c.String(nullable: false, maxLength: 128),
                        ResourceId = c.String(nullable: false, maxLength: 128),
                        ResourceField = c.String(nullable: false, maxLength: 32),
                        ResourceValue = c.String(nullable: false),
                        LanguageCode = c.String(nullable: false, maxLength: 4),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Localizations");
        }
    }
}

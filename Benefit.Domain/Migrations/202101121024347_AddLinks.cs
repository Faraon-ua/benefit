namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLinks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Url = c.String(nullable: false, maxLength: 400),
                        ExportImportId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExportImports", t => t.ExportImportId)
                .Index(t => t.ExportImportId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Links", "ExportImportId", "dbo.ExportImports");
            DropIndex("dbo.Links", new[] { "ExportImportId" });
            DropTable("dbo.Links");
        }
    }
}

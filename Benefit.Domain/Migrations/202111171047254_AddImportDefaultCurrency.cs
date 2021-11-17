namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddImportDefaultCurrency : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExportImports", "DefaultCurrencyId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ExportImports", "DefaultCurrencyId");
            AddForeignKey("dbo.ExportImports", "DefaultCurrencyId", "dbo.Currencies", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExportImports", "DefaultCurrencyId", "dbo.Currencies");
            DropIndex("dbo.ExportImports", new[] { "DefaultCurrencyId" });
            DropColumn("dbo.ExportImports", "DefaultCurrencyId");
        }
    }
}

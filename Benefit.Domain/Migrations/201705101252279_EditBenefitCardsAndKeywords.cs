namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class EditBenefitCardsAndKeywords : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Sellers", "KeyWords", "SearchTags");
            DropIndex("dbo.BenefitCards", new[] { "Id" });
            DropIndex("dbo.BenefitCards", new[] { "NfcCode" });
            DropPrimaryKey("dbo.BenefitCards");
            AddColumn("dbo.Sellers", "AltText", c => c.String(maxLength: 100));
            AddColumn("dbo.Products", "AltText", c => c.String(maxLength: 100));
            AddPrimaryKey("dbo.BenefitCards", new[] { "Id", "NfcCode" });
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Sellers", "SearchTags", "KeyWords");
            DropPrimaryKey("dbo.BenefitCards");
            DropColumn("dbo.Products", "AltText");
            DropColumn("dbo.Sellers", "AltText");
            AddPrimaryKey("dbo.BenefitCards", "Id");
            CreateIndex("dbo.BenefitCards", "NfcCode", unique: true);
            CreateIndex("dbo.BenefitCards", "Id", unique: true);
        }
    }
}

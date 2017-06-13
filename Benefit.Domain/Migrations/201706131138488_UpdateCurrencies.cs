namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCurrencies : DbMigration
    {
        public override void Up()
        {
            Sql("Update Currencies set Provider = '1'");
            AlterColumn("dbo.Currencies", "Provider", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Currencies", "Provider", c => c.String(nullable: false, maxLength: 16));
        }
    }
}

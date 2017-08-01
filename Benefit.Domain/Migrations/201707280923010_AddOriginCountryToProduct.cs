namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddOriginCountryToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "OriginCountry", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "OriginCountry");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductBarcode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Barcode", c => c.Long());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "Barcode");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddImageToProductOption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductOptions", "Image", c => c.String(maxLength: 100));
            AddColumn("dbo.ProductOptions", "IsVariant", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ProductOptions", "Order", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProductOptions", "Order", c => c.Int(nullable: false));
            DropColumn("dbo.ProductOptions", "IsVariant");
            DropColumn("dbo.ProductOptions", "Image");
        }
    }
}

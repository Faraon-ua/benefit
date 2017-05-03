namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditableProductOptionAmount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductOptions", "EditableAmount", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductOptions", "EditableAmount");
        }
    }
}

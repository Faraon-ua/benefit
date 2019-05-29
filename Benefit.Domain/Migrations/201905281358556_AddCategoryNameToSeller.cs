namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryNameToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "CategoryName", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "CategoryName");
        }
    }
}

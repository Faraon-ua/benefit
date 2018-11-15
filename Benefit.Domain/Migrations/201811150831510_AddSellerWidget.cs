namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerWidget : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Widget", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "Widget");
        }
    }
}

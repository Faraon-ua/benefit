namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Title", c => c.String(maxLength: 80));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "Title");
        }
    }
}

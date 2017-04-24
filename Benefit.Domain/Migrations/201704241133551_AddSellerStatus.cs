namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Status", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "Status");
        }
    }
}

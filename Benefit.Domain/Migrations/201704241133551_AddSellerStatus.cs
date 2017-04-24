namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSellerStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Status", c => c.Int(nullable: false, defaultValue: 0));            
            AlterColumn("dbo.Reviews", "Message", c => c.String(nullable: false, maxLength: 512));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Reviews", "Message", c => c.String(maxLength: 512));
            DropColumn("dbo.Sellers", "Status");
        }
    }
}

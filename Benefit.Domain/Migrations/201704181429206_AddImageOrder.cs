namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddImageOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Images", "Order", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Images", "Order");
        }
    }
}

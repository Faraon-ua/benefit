namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShowCartOnOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "ShowCartOnOrder", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "ShowCartOnOrder");
        }
    }
}

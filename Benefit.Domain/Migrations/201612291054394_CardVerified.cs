namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CardVerified : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUsers", "IsCardVerified", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUsers", "IsCardVerified");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLogRequestsToSellers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "LogRequests", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "LogRequests");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalIdToOrderProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderProducts", "ExternalId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderProducts", "ExternalId");
        }
    }
}

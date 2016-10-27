namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddressLineNulable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Addresses", "AddressLine", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Addresses", "AddressLine", c => c.String(nullable: false, maxLength: 256));
        }
    }
}

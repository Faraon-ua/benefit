namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNameToNotificationChannel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationChannels", "Name", c => c.String(maxLength: 128));
            AddColumn("dbo.Products", "AvailabilityState", c => c.Int(nullable: false));
            Sql("Update Products set AvailabilityState = 1 Where AvailableAmount is null");
            Sql("Update Products set AvailabilityState = 2 Where AvailableAmount = 0");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "AvailabilityState");
            DropColumn("dbo.NotificationChannels", "Name");
        }
    }
}

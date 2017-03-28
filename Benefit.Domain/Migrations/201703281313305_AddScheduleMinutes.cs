namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddScheduleMinutes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Schedules", "StartMinutes", c => c.Int());
            AddColumn("dbo.Schedules", "EndMinutes", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Schedules", "EndMinutes");
            DropColumn("dbo.Schedules", "StartMinutes");
        }
    }
}

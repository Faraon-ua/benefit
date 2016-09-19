namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSchedules : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Schedules",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ScheduleType = c.Int(nullable: false),
                        Day = c.Int(nullable: false),
                        StartHour = c.Int(),
                        EndHour = c.Int(),
                        SellerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .Index(t => t.SellerId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Schedules", "SellerId", "dbo.Sellers");
            DropIndex("dbo.Schedules", new[] { "SellerId" });
            DropTable("dbo.Schedules");
        }
    }
}

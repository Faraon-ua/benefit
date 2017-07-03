namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationChannels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationChannels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Address = c.String(maxLength: 128),
                        ChannelType = c.Int(nullable: false),
                        SellerId = c.String(maxLength: 128),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sellers", t => t.SellerId)
                .ForeignKey("dbo.ApplicationUsers", t => t.UserId)
                .Index(t => t.SellerId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationChannels", "UserId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.NotificationChannels", "SellerId", "dbo.Sellers");
            DropIndex("dbo.NotificationChannels", new[] { "UserId" });
            DropIndex("dbo.NotificationChannels", new[] { "SellerId" });
            DropTable("dbo.NotificationChannels");
        }
    }
}

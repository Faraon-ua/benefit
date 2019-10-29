namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderUserInfo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Orders", "UserId", "dbo.ApplicationUsers");
            DropIndex("dbo.Orders", new[] { "UserId" });
            AddColumn("dbo.Orders", "ExternalId", c => c.String());
            AddColumn("dbo.Orders", "UserName", c => c.String());
            AddColumn("dbo.Orders", "UserPhone", c => c.String(maxLength: 20));
            AlterColumn("dbo.Orders", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Orders", "UserId");
            AddForeignKey("dbo.Orders", "UserId", "dbo.ApplicationUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "UserId", "dbo.ApplicationUsers");
            DropIndex("dbo.Orders", new[] { "UserId" });
            AlterColumn("dbo.Orders", "UserId", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Orders", "UserPhone");
            DropColumn("dbo.Orders", "UserName");
            DropColumn("dbo.Orders", "ExternalId");
            CreateIndex("dbo.Orders", "UserId");
            AddForeignKey("dbo.Orders", "UserId", "dbo.ApplicationUsers", "Id", cascadeDelete: true);
        }
    }
}

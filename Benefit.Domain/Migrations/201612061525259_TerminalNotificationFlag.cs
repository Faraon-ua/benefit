namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TerminalNotificationFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "TerminalOrderNotification", c => c.Boolean(nullable: false));
            AddColumn("dbo.Sellers", "IsCashPaymentActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.Orders", "PersonnelId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "PersonnelId", c => c.String(maxLength: 128));
            DropColumn("dbo.Sellers", "IsCashPaymentActive");
            DropColumn("dbo.Sellers", "TerminalOrderNotification");
        }
    }
}

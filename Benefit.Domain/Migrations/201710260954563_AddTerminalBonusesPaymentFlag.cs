namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTerminalBonusesPaymentFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "TerminalBonusesPaymentActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "TerminalBonusesPaymentActive");
        }
    }
}

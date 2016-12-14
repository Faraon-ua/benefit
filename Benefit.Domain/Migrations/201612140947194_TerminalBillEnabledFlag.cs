namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TerminalBillEnabledFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "TerminalBillEnabled", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "TerminalBillEnabled");
        }
    }
}

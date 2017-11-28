namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TerminalKeyboardEnabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "TerminalKeyboardEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "TerminalKeyboardEnabled");
        }
    }
}

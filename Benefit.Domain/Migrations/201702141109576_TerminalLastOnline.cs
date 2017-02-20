namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TerminalLastOnline : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "TerminalLastOnline", c => c.DateTime(nullable: false, storeType: "datetime2"));
            AlterColumn("dbo.Sellers", "TerminalPassword", c => c.String(maxLength: 32));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Sellers", "TerminalPassword", c => c.String(maxLength: 16));
            DropColumn("dbo.Sellers", "TerminalLastOnline");
        }
    }
}

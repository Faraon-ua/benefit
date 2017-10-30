namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTerminalLicense : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "TerminalLicense", c => c.String(maxLength: 32));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "TerminalLicense");
        }
    }
}

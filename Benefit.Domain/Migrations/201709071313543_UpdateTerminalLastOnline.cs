namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTerminalLastOnline : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Sellers", "TerminalLastOnline", c => c.DateTime(storeType: "datetime2"));
            Sql("Update Sellers set TerminalLastOnline = null");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Sellers", "TerminalLastOnline", c => c.DateTime(nullable: false));
        }
    }
}

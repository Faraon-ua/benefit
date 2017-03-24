namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendNfcLength : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ApplicationUsers", new[] { "NFCCardNumber" });
            AlterColumn("dbo.ApplicationUsers", "NFCCardNumber", c => c.String(maxLength: 16));
            CreateIndex("dbo.ApplicationUsers", "NFCCardNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ApplicationUsers", new[] { "NFCCardNumber" });
            AlterColumn("dbo.ApplicationUsers", "NFCCardNumber", c => c.String(maxLength: 10));
            CreateIndex("dbo.ApplicationUsers", "NFCCardNumber");
        }
    }
}

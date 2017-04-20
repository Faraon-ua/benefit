namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CardRequiredPersonell : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Personnels", "NFCCardNumber", c => c.String(nullable: false, maxLength: 10));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Personnels", "NFCCardNumber", c => c.String(maxLength: 10));
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendBenefitCardId : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.BenefitCards");
            AlterColumn("dbo.BenefitCards", "Id", c => c.String(nullable: false, maxLength: 20));
            AddPrimaryKey("dbo.BenefitCards", new[] { "Id", "NfcCode" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.BenefitCards");
            AlterColumn("dbo.BenefitCards", "Id", c => c.String(nullable: false, maxLength: 10));
            AddPrimaryKey("dbo.BenefitCards", new[] { "Id", "NfcCode" });
        }
    }
}

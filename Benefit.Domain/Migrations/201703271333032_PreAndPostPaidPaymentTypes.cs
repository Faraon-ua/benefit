namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PreAndPostPaidPaymentTypes : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Sellers", "IsAgreementPaymentActive", "IsPrePaidPaymentActive");
            AddColumn("dbo.Sellers", "IsPostPaidPaymentActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Sellers", "IsPrePaidPaymentActive", "IsAgreementPaymentActive");
            DropColumn("dbo.Sellers", "IsPostPaidPaymentActive");
        }
    }
}

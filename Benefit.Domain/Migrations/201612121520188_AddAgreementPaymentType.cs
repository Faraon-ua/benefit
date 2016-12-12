namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAgreementPaymentType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "IsAgreementPaymentActive", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Orders", "LastModified", c => c.DateTime(nullable: false, storeType: "datetime2"));
            AddColumn("dbo.Orders", "LastModifiedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "LastModifiedBy");
            DropColumn("dbo.Orders", "LastModified");
            DropColumn("dbo.Sellers", "IsAgreementPaymentActive");
        }
    }
}

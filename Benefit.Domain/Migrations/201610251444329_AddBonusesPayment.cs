namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBonusesPayment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "IsBonusesPaymentActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Sellers", "IsAcquiringActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "IsAcquiringActive");
            DropColumn("dbo.Sellers", "IsBonusesPaymentActive");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAddressAndNFCToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUsers", "NFCCardNumber", c => c.String(maxLength: 10));
            AddColumn("dbo.ApplicationUsers", "FinancialPassword", c => c.String(maxLength: 8));
            AddColumn("dbo.ApplicationUsers", "RegionId", c => c.Int(nullable: false, defaultValue: 400000));            
            AddColumn("dbo.ApplicationUsers", "Address", c => c.String(maxLength: 128));
            AddColumn("dbo.ApplicationUsers", "CurrentHandlingBonusAccount", c => c.Double(nullable: false));
            CreateIndex("dbo.ApplicationUsers", "NFCCardNumber");
            CreateIndex("dbo.ApplicationUsers", "RegionId");
            AddForeignKey("dbo.ApplicationUsers", "RegionId", "dbo.Regions", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationUsers", "RegionId", "dbo.Regions");
            DropIndex("dbo.ApplicationUsers", new[] { "RegionId" });
            DropIndex("dbo.ApplicationUsers", new[] { "NFCCardNumber" });
            DropColumn("dbo.ApplicationUsers", "CurrentHandlingBonusAccount");
            DropColumn("dbo.ApplicationUsers", "Address");
            DropColumn("dbo.ApplicationUsers", "RegionId");
            DropColumn("dbo.ApplicationUsers", "FinancialPassword");
            DropColumn("dbo.ApplicationUsers", "NFCCardNumber");
        }
    }
}

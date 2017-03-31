namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBenefitFamily : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BenefitCards", "IsTrinket", c => c.Boolean(nullable: false));
            AddColumn("dbo.BenefitCards", "HolderName", c => c.String(maxLength: 64));
            AddColumn("dbo.BenefitCards", "UserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.BenefitCards", "NfcCode", c => c.String(nullable: false, maxLength: 16));
            CreateIndex("dbo.BenefitCards", "Id", unique: true);
            CreateIndex("dbo.BenefitCards", "NfcCode", unique: true);
            CreateIndex("dbo.BenefitCards", "UserId");
            AddForeignKey("dbo.BenefitCards", "UserId", "dbo.ApplicationUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BenefitCards", "UserId", "dbo.ApplicationUsers");
            DropIndex("dbo.BenefitCards", new[] { "UserId" });
            DropIndex("dbo.BenefitCards", new[] { "NfcCode" });
            DropIndex("dbo.BenefitCards", new[] { "Id" });
            AlterColumn("dbo.BenefitCards", "NfcCode", c => c.String(nullable: false, maxLength: 10));
            DropColumn("dbo.BenefitCards", "UserId");
            DropColumn("dbo.BenefitCards", "HolderName");
            DropColumn("dbo.BenefitCards", "IsTrinket");
        }
    }
}

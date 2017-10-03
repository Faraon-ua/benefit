namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReferalToBenefitCard : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BenefitCards", "ReferalUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.BenefitCards", "ReferalUserId");
            AddForeignKey("dbo.BenefitCards", "ReferalUserId", "dbo.ApplicationUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BenefitCards", "ReferalUserId", "dbo.ApplicationUsers");
            DropIndex("dbo.BenefitCards", new[] { "ReferalUserId" });
            DropColumn("dbo.BenefitCards", "ReferalUserId");
        }
    }
}

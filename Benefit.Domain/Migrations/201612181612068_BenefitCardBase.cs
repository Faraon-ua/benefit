namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BenefitCardBase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BenefitCards",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 10),
                        NfcCode = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BenefitCards");
        }
    }
}

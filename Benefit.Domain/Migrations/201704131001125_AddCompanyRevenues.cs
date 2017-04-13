namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddCompanyRevenues : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompanyRevenues",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Stamp = c.DateTime(nullable: false, storeType: "datetime2"),
                        TotalBonuses = c.Double(nullable: false),
                        TotalHangingBonuses = c.Double(nullable: false),
                        TotalEarnedBonuses = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.CompanyRevenues");
        }
    }
}

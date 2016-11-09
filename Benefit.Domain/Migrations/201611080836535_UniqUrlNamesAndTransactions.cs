namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UniqUrlNamesAndTransactions : DbMigration
    {
        public override void Up()
        {
            var renameDuplicates = @"With Dups As
                                    (
                                    Select Id, UrlName
                                        , Row_Number() Over ( Partition By UrlName Order By Id ) As Rnk
                                    From Categories
                                    )
                                Update Categories
                                Set UrlName = T.UrlName + Case
                                                    When D.Rnk > 1 Then '(' + Cast(D.Rnk As varchar(10)) + ')'
                                                    Else ''
                                                    End
                                From Categories As T
                                    Join Dups As D
                                        On D.Id = T.Id";
            Sql(renameDuplicates);

            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Type = c.Int(nullable: false),
                        Bonuses = c.Double(),
                        BonusesBalans = c.Double(),
                        Time = c.DateTime(nullable: false),
                        PayerId = c.String(maxLength: 128),
                        PayeeId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ApplicationUsers", t => t.PayeeId)
                .ForeignKey("dbo.ApplicationUsers", t => t.PayerId)
                .Index(t => t.PayerId)
                .Index(t => t.PayeeId);

            AddColumn("dbo.ApplicationUsers", "BonusAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "TotalBonusAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "HangingBonusAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "PointsAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "HangingPointsAccount", c => c.Double(nullable: false));
            CreateIndex("dbo.Sellers", "UrlName", unique: true);
            CreateIndex("dbo.Products", "UrlName", unique: true);
            CreateIndex("dbo.Categories", "UrlName", unique: true);
            DropColumn("dbo.ApplicationUsers", "CurrentHandlingBonusAccount");
            DropColumn("dbo.ApplicationUsers", "CurrentPointsAccount");
        }

        public override void Down()
        {
            AddColumn("dbo.ApplicationUsers", "CurrentPointsAccount", c => c.Double(nullable: false));
            AddColumn("dbo.ApplicationUsers", "CurrentHandlingBonusAccount", c => c.Double(nullable: false));
            DropForeignKey("dbo.Transactions", "PayerId", "dbo.ApplicationUsers");
            DropForeignKey("dbo.Transactions", "PayeeId", "dbo.ApplicationUsers");
            DropIndex("dbo.Transactions", new[] { "PayeeId" });
            DropIndex("dbo.Transactions", new[] { "PayerId" });
            DropIndex("dbo.Categories", new[] { "UrlName" });
            DropIndex("dbo.Products", new[] { "UrlName" });
            DropIndex("dbo.Sellers", new[] { "UrlName" });
            DropColumn("dbo.ApplicationUsers", "HangingPointsAccount");
            DropColumn("dbo.ApplicationUsers", "PointsAccount");
            DropColumn("dbo.ApplicationUsers", "HangingBonusAccount");
            DropColumn("dbo.ApplicationUsers", "TotalBonusAccount");
            DropColumn("dbo.ApplicationUsers", "BonusAccount");
            DropTable("dbo.Transactions");
        }
    }
}

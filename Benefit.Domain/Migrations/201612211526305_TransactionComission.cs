namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class TransactionComission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "Commission", c => c.Double(nullable: false));
            AlterColumn("dbo.Transactions", "Bonuses", c => c.Double(nullable: false));
            AlterColumn("dbo.Transactions", "BonusesBalans", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Transactions", "BonusesBalans", c => c.Double());
            AlterColumn("dbo.Transactions", "Bonuses", c => c.Double());
            DropColumn("dbo.Transactions", "Commission");
        }
    }
}

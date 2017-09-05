namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveComission : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Transactions", "Commission");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Transactions", "Commission", c => c.Double(nullable: false));
        }
    }
}

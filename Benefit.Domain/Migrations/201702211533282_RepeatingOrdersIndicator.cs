namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RepeatingOrdersIndicator : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "RepeatingTransactionInterval", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "RepeatingTransactionInterval");
        }
    }
}

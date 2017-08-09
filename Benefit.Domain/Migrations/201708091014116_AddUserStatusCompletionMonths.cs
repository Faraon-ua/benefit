namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserStatusCompletionMonths : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUsers", "StatusCompletionMonths", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUsers", "StatusCompletionMonths");
        }
    }
}

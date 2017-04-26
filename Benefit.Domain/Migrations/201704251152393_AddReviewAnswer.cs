namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReviewAnswer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reviews", "ReviewId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Reviews", "ReviewId");
            AddForeignKey("dbo.Reviews", "ReviewId", "dbo.Reviews", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reviews", "ReviewId", "dbo.Reviews");
            DropIndex("dbo.Reviews", new[] { "ReviewId" });
            DropColumn("dbo.Reviews", "ReviewId");
        }
    }
}

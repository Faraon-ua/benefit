namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddModeratorAsignee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ModerationAssigneeId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Products", "ModerationAssigneeId");
            AddForeignKey("dbo.Products", "ModerationAssigneeId", "dbo.ApplicationUsers", "Id");
            Sql("Insert into IdentityRoles values ('3535f7b2-4d74-4e90-9ca2-8d34807b41ef','ProductsModerator')");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "ModerationAssigneeId", "dbo.ApplicationUsers");
            DropIndex("dbo.Products", new[] { "ModerationAssigneeId" });
            DropColumn("dbo.Products", "ModerationAssigneeId");
        }
    }
}

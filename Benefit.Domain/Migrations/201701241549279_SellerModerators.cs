namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SellerModerators : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Personnels", "UserId", c => c.String(maxLength: 128));
            AddColumn("dbo.Personnels", "RoleName", c => c.String(maxLength: 20));
            CreateIndex("dbo.Personnels", "UserId");
            AddForeignKey("dbo.Personnels", "UserId", "dbo.ApplicationUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Personnels", "UserId", "dbo.ApplicationUsers");
            DropIndex("dbo.Personnels", new[] { "UserId" });
            DropColumn("dbo.Personnels", "RoleName");
            DropColumn("dbo.Personnels", "UserId");
        }
    }
}

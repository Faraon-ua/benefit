namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserAvatar : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUsers", "Avatar", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUsers", "Avatar");
        }
    }
}

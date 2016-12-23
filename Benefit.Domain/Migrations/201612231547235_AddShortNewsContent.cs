namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShortNewsContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InfoPages", "ShortContent", c => c.String(maxLength: 512));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InfoPages", "ShortContent");
        }
    }
}

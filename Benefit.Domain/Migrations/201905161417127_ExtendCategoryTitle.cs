namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendCategoryTitle : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Categories", "Title", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Categories", "Title", c => c.String(maxLength: 100));
        }
    }
}

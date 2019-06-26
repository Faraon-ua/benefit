namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLastModifiedByToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "LastModifiedBy", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "LastModifiedBy");
        }
    }
}

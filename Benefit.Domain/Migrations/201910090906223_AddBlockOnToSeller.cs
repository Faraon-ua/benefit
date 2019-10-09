namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBlockOnToSeller : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "BlockOn", c => c.DateTime(storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "BlockOn");
        }
    }
}

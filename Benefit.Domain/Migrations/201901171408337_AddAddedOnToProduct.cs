namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAddedOnToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "AddedOn", c => c.DateTime(nullable: false, storeType: "datetime2", defaultValue: DateTime.Now.AddDays(-15) ));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "AddedOn");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductSkuOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderProducts", "ProductSku", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderProducts", "ProductSku");
        }
    }
}

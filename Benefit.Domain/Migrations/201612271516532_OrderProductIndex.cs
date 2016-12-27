namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class OrderProductIndex : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderProducts", "Index", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderProducts", "Index");
        }
    }
}

namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "ShippingDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "ShippingDescription");
        }
    }
}

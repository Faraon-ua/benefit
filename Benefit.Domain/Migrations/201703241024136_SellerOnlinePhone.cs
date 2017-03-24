namespace Benefit.Domain.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class SellerOnlinePhone : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "OnlineOrdersPhone", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "OnlineOrdersPhone");
        }
    }
}

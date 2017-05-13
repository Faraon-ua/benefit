namespace Benefit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SellerLatLong : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sellers", "Latitude", c => c.Double());
            AddColumn("dbo.Sellers", "Longitude", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sellers", "Longitude");
            DropColumn("dbo.Sellers", "Latitude");
        }
    }
}
